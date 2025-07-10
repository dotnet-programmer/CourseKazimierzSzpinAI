using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Components.Pages.CulinaryAssistant;

public partial class CulinaryAssistant(IFileService fileService, IAzureVisionHttpRepository azureVision, IOpenAiHttpRepository openAi)
{
	// do wyświetlenia wybranego zdjecia na widoku bez zapisywania go na dysku
	private string? _imageDataUrl;

	// wybrany plik w postaci tablicy bajtów
	private byte[]? _selectedFileContent;

	// oczekiwanie na ładowanie danych
	private bool _isLoading = false;

	// do wyświetlenia na widoku wszystkich rozpoznanych składników znajdujących się na przesłanym zdjęciu
	private List<string> _ingredients = [];

	// możliwość wprowadzenia nowych składników przez użytkownika - pole do pobierania wartości z formularza
	private string? _ingredient;

	// zdarzenie wywołane po wyborze pliku na widoku
	private async Task OnFileSelectedAsync(InputFileChangeEventArgs e)
	{
		try
		{
			_selectedFileContent = await fileService.ReadInputAsBytesAsync(e.File);
			_imageDataUrl = fileService.GetBase64String(e.File.ContentType, _selectedFileContent);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd podczas wczytywania zdjęcia: {ex.Message}");
			throw;
		}
	}

	// metoda do rozpoznawania składników na zdjęciu
	private async Task IdentifyIngredientsAsync()
	{
		// jeśli żaden plik nie został wybrany to wyjdź z metody
		if (_selectedFileContent == null)
		{
			return;
		}

		_isLoading = true;
		_ingredients.Clear();

		try
		{
			// wywołanie AzureAi który rozpozna obiekty na zdjęciu
			var result = await azureVision.GetImageInfoAsync(_selectedFileContent);

			// pomocnicza zmienna do przechowywania nazw obiektów, które zwrócił serwis Azure
			List<string> objects = [];

			// jeżeli został zwrócony jakiś opis zdjęcia to też zostanie dodany do listy
			if (!string.IsNullOrWhiteSpace(result?.CaptionResult?.Text))
			{
				objects.Add(result.CaptionResult.Text);
			}

			// dodanie do listy składników wszystkich tagów wygenerowanych przez Azure
			if (result?.TagsResult != null)
			{
				objects.AddRange(result.TagsResult.Values
					.Select(t => t.Name)
					.Where(name => !string.IsNullOrWhiteSpace(name)));
			}

			// na podstawie opisu i tagów będzie pobierana informacja o składnikach
			// w tym miejscu wartości są po angielsku, więc trzeba to przetłumaczyć
			// dodatkowo nie wszystkie tagi będą składnikami kulinarnymi, dlatego dodatkowo OpenAi musi sprawdzć czy dany tag jest składnikiem kulinarnym, jeżeli tak to zostanie przetłumaczony i dodany

			// string zawierający opis i wszystkie tagi, który zostanie wysłany w prompcie do OpenAi
			string items = string.Join(",", objects);

			// budowanie prompta - musi mieć dużo szczegółów żeby odpowiedź była precyzyjna
			var prompt = $@"
Przesyłam Ci listę ciągów tekstowych. Twoim zadaniem jest sprawdzić, czy każdy element listy jest konkretnym składnikiem kulinarnym, czyli pojedynczym produktem spożywczym, który można wykorzystać do przygotowania posiłku (np. pomidor, chleb, jabłko, papryka, jajko, ser). Elementy o charakterze ogólnym lub kategorycznym (np. 'grupa żywności', 'warzywo', 'produkty naturalne', 'odżywianie wegańskie') należy pominąć.Jeśli element spełnia warunki, przetłumacz go na język polski. Na końcu zwróć listę tylko tych konkretnych składników, oddzielonych przecinkami, bez dodatkowych słów ani opisu. Moja lista:
{items}
Poproszę tylko o nową listę składników, nic więcej nie dodawaj. Jeżeli Twoim zdaniem nie ma żadnego składnika, który spełnia wszystkie wymagania, to zwróć tylko jeden znak '-', w przeciwnym przypadku lista składników oddzielona przecinkami. Bez żadnego dodatkowo komentarza.
Przykładowy format wyjścia: jabłko,malina,papryka,szynka.
Odpowiedz w języku polskim.";

			// wysłanie prompta do OpenAi i odebranie odpowiedzi w openAiAnswer, odpowiedź w formacie 1 stringa z listą składników oddzielonych przecinkami
			var openAiAnswer = await openAi.AskOpenAiAsync(prompt);

			// rozdzielenie pojedynczego stringa na listę składników
			_ingredients = openAiAnswer
				.Split(",", StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim('-').Trim('.').Trim())
				.ToList();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd analizy obrazu: {ex.Message}");
		}
		finally
		{
			_isLoading = false;
		}
	}

	// metoda do usuwania wybranego składnika z listy wszystkich składników
	private void RemoveIngredient(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			_ingredients.Remove(name);
		}

		//if (string.IsNullOrWhiteSpace(name))
		//{
		//	return;
		//}
		//var ingredientToDelete = _ingredients.FirstOrDefault(x => x == name);
		//if (string.IsNullOrWhiteSpace(ingredientToDelete))
		//{
		//	return;
		//}
		//_ingredients.Remove(ingredientToDelete);
	}

	// metoda do dodania nowego składnika do listy przez użytkownika
	private void AddIngredient()
	{
		if (!string.IsNullOrWhiteSpace(_ingredient))
		{
			_ingredients.Add(_ingredient);
			_ingredient = null;
		}
	}
}