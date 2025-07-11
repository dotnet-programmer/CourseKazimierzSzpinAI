using IntelligentApp.Helpers;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.ContentAssistant;
using IntelligentApp.Services.Interfaces;
using Microsoft.JSInterop;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.ContentAssistant;

public partial class ContentAssistant(IJSRuntime JS, IFileService fileService, IAzureSpeechHttpRepository azureSpeech, IOpenAiHttpRepository openAi, IAzureAiHttpRepository azureAi)
{
	// stała wartość, żeby nowy obiekt odróżniał się od innych
	private const int NewProductId = -1;

	private bool _isLoading = false;
	private bool _isRecording = false;
	private string? _userProductDesc;
	private byte[]? _audio;
	private string? _generatedName;
	private string? _generatedDescription;
	private string? _notes;
	private List<ChatMessage> _nameMessages = [];
	private List<ChatMessage> _descriptionMessages = [];
	private string? _imageDataUrl;
	private List<string> _keyPhrases = [];

	// lista produktów wczytanych z pliku .csv
	private List<ProductData>? _allProducts;

	// lista wektoró cech dla każdego produktu
	private List<ProductVector>? _productVectors;
	
	// lista z podobnymi produktami, które będą wyświetlane na widoku
	private List<SimilarProduct>? _similarProducts;

	private async Task StartRecordingAsync()
	{
		_isRecording = true;
		_userProductDesc = null;
		_audio = null;
		await JS.InvokeVoidAsync("audioRecorder.startRecording");
	}

	private async Task StopRecordingAsync()
	{
		var base64 = await JS.InvokeAsync<string>("audioRecorder.stopRecording");

		if (!string.IsNullOrWhiteSpace(base64))
		{
			_audio = Convert.FromBase64String(base64);
		}

		if (_audio == null)
		{
			return;
		}

		var wavFile = await fileService.ConvertWebmToWavAsync(_audio);
		_userProductDesc = await azureSpeech.GetTextAsync(wavFile);
		_isRecording = false;
	}

	private async Task GenerateAsync()
	{
		if (string.IsNullOrWhiteSpace(_userProductDesc))
		{
			return;
		}

		_isLoading = true;

		await GenerateNameAsync();
		await GenerateDescriptionAsync();
		await GenerateImageAsync();
		await GenerateKeyPhrasesAsync();

		PrepareProductsVectors();
		ShowRecommendations();

		_isLoading = false;
	}

	private async Task GenerateNameAsync()
	{
		var prompt = $@"Wygeneruj krótką, chwytliwą nazwę produktu na podstawie poniższych informacji: {_userProductDesc} Użyj języka polskiego. Podaj tylko samą nazwę, bez dodatkowego wyjaśnienia.";

		// dodanie prompta użytkownika do historii rozmowy
		_nameMessages.Add(new ChatMessage { Role = "user", Content = prompt });
		_generatedName = await openAi.AskOpenAiWithHistoryAsync(_nameMessages);
		// dodanie odpowiedzi OpenAi do historii rozmowy
		_nameMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedName });
	}

	private async Task GenerateDescriptionAsync()
	{
		var prompt = $@"Na podstawie tych informacji o produkcie: {_userProductDesc} Stwórz angażujący, marketingowy opis produktu w języku polskim. Napisz w taki sposób, by był atrakcyjny dla potencjalnego klienta. Opis ma zawierać około 100 znaków.";

		_descriptionMessages.Add(new ChatMessage { Role = "user", Content = prompt });
		_generatedDescription = await openAi.AskOpenAiWithHistoryAsync(_descriptionMessages);
		_descriptionMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedDescription });
	}

	private async Task ChangeNameAsync()
	{
		_nameMessages.Add(new ChatMessage { Role = "user", Content = _notes });
		_generatedName = await openAi.AskOpenAiWithHistoryAsync(_nameMessages);
		_nameMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedName });
	}

	private async Task ChangeDescriptionAsync()
	{
		_descriptionMessages.Add(new ChatMessage { Role = "user", Content = _notes });
		_generatedDescription = await openAi.AskOpenAiWithHistoryAsync(_descriptionMessages);
		_descriptionMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedDescription });
	}

	private async Task GenerateImageAsync()
	{
		var dallePrompt = $"Stwórz mi prompta, który mogę wysłać do DALLE, tak żeby zostało wygenerowane ładne zdjęcie mojego produktu o nazwie: {_generatedName}. Opis produktu: {_generatedDescription}. To zdjęcie będę chciał ustawić jako miniaturka w moim sklepie internetowym.";
		_imageDataUrl = await openAi.GenerateImageAsync(dallePrompt);
	}

	private async Task GenerateKeyPhrasesAsync()
	{
		if (_generatedDescription != null && _generatedDescription.Any())
		{
			_keyPhrases = await azureAi.ExtractKeyPhrasesAsync(_generatedDescription);
		}
	}

	private void PrepareProductsVectors()
	{
		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<ProductData>(
			 path: fileService.GetFilePath("data", "content-assistant", "products_shop.csv"),
			 hasHeader: true,
			 separatorChar: ',',
			 allowQuoting: true);

		// uzupełnienie listy z produktami danymi z pliku .csv
		_allProducts = mlContext.Data
		   .CreateEnumerable<ProductData>(dataView, reuseRowObject: false)
		   .ToList();

		// budowanie listy podobnych produktów do tego nowego produktu, którego jeszcze nie ma w pliku .csv, a więc i w dataView
		// dlatego trzeba utworzyć nowy obiekt tego produktu i dodać go do listy wszystkich produktów, żeby zbudować jego wektor cech żeby porównywać go z innymi produktami
		_allProducts.Add(new ProductData
		{
			ProductId = NewProductId,
			Name = _generatedName,
			Description = _generatedDescription
		});

		// jeszcze raz tworzy się dataView, tym razem z dołączonym już nowym produktem
		dataView = mlContext.Data.LoadFromEnumerable(_allProducts);

		var pipeline = mlContext.Transforms
			// złączenie nazwy i opisu w kolumnie inputu TextInput
			.Concatenate("TextInput", "Name", "Description")
			// wprowadzenie inputu w TextInput do transformacji i output będzie w Features
			.Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: "TextInput"));

		// trenowanie
		var transform = pipeline.Fit(dataView);
		
		// transfromacja
		var transformData = transform.Transform(dataView);

		// tworzenie tablicy Features, gdzie są tablice floatów, czyli wetory cech
		var features = mlContext.Data.CreateEnumerable<TransformedProduct>(transformData, reuseRowObject: false);

		// złączenie powstałego wektora cech ze wszystkimi produktami, gdzie wektor cech będzie przypisany do Id produktu
		_productVectors = features
			.Zip(_allProducts, (f, m) => new ProductVector
			{
				ProductId = m.ProductId,
				Features = f.Features
			})
			.ToList();
	}

	private void ShowRecommendations()
		=> _similarProducts = GetSimilarProducts(NewProductId, 3);

	// productId - do tego będą porównywane inne produkty
	private List<SimilarProduct> GetSimilarProducts(float productId, int topN = 3)
	{
		if (_productVectors == null)
		{
			return [];
		}

		// w liście z wektorami przypisanymi do produktów szukany jest produkt z którym będzie porównywanie
		var targetProduct = _productVectors.FirstOrDefault(m => m.ProductId == productId);

		if (targetProduct == null)
		{
			return [];
		}

		// tutaj będą podobne produkty
		List<SimilarProduct> similarities = [];

		foreach (var prod in _productVectors)
		{
			// bez porównywania tych samych produktów, muszą być zaproponowane inne
			if (prod.ProductId == productId)
			{
				continue;
			}

			var sim = RecommendationSimilarity.CosineSimilarity(targetProduct.Features, prod.Features);

			var productInfo = _allProducts?.First(x => x.ProductId == prod.ProductId);
			similarities.Add(new SimilarProduct { Product = productInfo, Similarity = sim });
		}

		return similarities.OrderByDescending(s => s.Similarity).Take(topN).ToList();
	}
}