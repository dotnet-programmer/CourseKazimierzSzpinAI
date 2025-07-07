using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Components.Pages;

public partial class ImageAnalysis
{
	private List<string> _tags = [];
	private bool _isLoading = false;
	private bool _showResults = false;
	private string _description = string.Empty;
	private string _textFromImage = string.Empty;

	// do zapisania przekazanego pliku, żeby wykorzystać go w metodzie analizującej
	private byte[] _selectedFileContent;

	// adres URL do obrazu żeby wyświetlić go w komponencie IMG na stronie, obraz nie będzie zapisywany na serwerze
	private string _imageDataUrl;

	[Inject]
	public IFileReader FileReader { get; set; }

	[Inject]
	public IAzureAiHttpRepository AzureAiHttpRepository { get; set; }

	private async Task AnalyzeImageFromServer()
	{
		// pobranie tablicy bajtów z pliku obrazu
		var fileBytes = await FileReader.ReadImageAsBytes("nazwa.png");
		await AnalyzeImageAsync(fileBytes);
	}

	private async Task AnalyzeUserImageAsync()
	{
		if (_selectedFileContent != null)
		{
			await AnalyzeImageAsync(_selectedFileContent);
		}
	}

	private async Task AnalyzeImageAsync(byte[] fileBytes)
	{
		_isLoading = true;
		_showResults = false;
		_description = string.Empty;
		_tags.Clear();

		try
		{
			var result = await AzureAiHttpRepository.GetImageInfoAsync(fileBytes);

			// odczytanie opisu obrazu
			_description = result?.CaptionResult.Text ?? "";

			// odczytanie tagów obrazu
			if (result?.TagsResult != null)
			{
				_tags = result.TagsResult.Values
					.Select(x => x.Name)
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToList();
			}

			// odczytanie tekstu z obrazu
			// sprawdzenie czy udało się pobrać jakiś tekst ze zdjęcia
			if (result?.ReadResult?.Blocks != null)
			{
				foreach (var block in result.ReadResult.Blocks)
				{
					foreach (var line in block.Lines)
					{
						_textFromImage += $"Linia: {line.Text}{Environment.NewLine}";
					}
				}
			}
		}
		catch (Exception ex)
		{
			_description = $"Błąd analizy obrazu: {ex.Message}";
		}
		finally
		{
			_isLoading = false;
			_showResults = true;
		}
	}

	// w parametrze metody będzie przekazany plik obrazu wczytany przez użyszkodnika
	// plik jest odczytywany w pamięci, bez wysyłania na serwer
	private async Task OnFileSelectedAsync(InputFileChangeEventArgs e)
	{
		try
		{
			// pobranie pliku z argumentu
			var file = e.File;

			using MemoryStream ms = new();
			// odczytanie zawartości pliku i skopiowanie go do strumienia w pamięci
			// oddatkowo ustawiony maksymalny rozmiar pliku na 10 mb, jak będzie większy to zgłoszony wyjątek
			await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);

			// do zapisania przekazanego pliku jako tablicy bajtów, żeby wykorzystać go w metodzie analizującej
			_selectedFileContent = ms.ToArray();

			// zbudowanie adresu URL do pliku
			var base64 = Convert.ToBase64String(_selectedFileContent);
			_imageDataUrl = $"data:{file.ContentType};base64,{base64}";
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd podczas wczytywania pliku: {ex.Message}.");
			throw;
		}
	}
}