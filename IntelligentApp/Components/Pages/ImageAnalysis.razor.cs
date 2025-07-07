using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class ImageAnalysis
{
	private List<string> _tags = [];
	private bool _isLoading = false;
	private string _description = string.Empty;

	[Inject]
	public IFileReader FileReader { get; set; }

	[Inject]
	public IAzureAiHttpRepository AzureAiHttpRepository { get; set; }

	private async Task AnalyzeImageAsync()
	{
		_isLoading = true;
		_description = string.Empty;
		_tags.Clear();

		try
		{
			// pobranie tablicy bajtów z pliku obrazu
			var fileBytes = await FileReader.ReadImageAsBytes("nazwa.png");

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
		}
		catch (Exception ex)
		{
			_description = $"Błąd analizy obrazu: {ex.Message}";
		}
		finally
		{
			_isLoading = false;
		}
	}
}