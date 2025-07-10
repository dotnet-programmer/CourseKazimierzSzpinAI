using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.AzureAi;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.AzureAi_OpenAi;

public partial class OpinionsAnalysis
{
	private readonly List<Opinion> _opinions = [];
	
	private bool _isLoading = false;
	private bool _isAnalyzing = false;

	[Inject]
	protected IFileService FileService { get; set; } = default!;

	[Inject]
	protected IAzureAiHttpRepository AzureAiHttpRepository { get; set; } = default!;

	private async Task LoadOpinionsAsync()
	{
		_isLoading = true;

		_opinions.Clear();
		var lines = await FileService.ReadAllLinesAsync("azureai", "opinions.csv", '"');
		for (int i = 1; i < lines.Count; i++)
		{
			_opinions.Add(new() { Review = lines[i] });
		}

		_isLoading = false;
	}

	private async Task AnalyzeOneOpinionAsync(Opinion opinion)
	{
		_isAnalyzing = true;

		await AnalyzeOpinionAsync(opinion);

		_isAnalyzing = false;
	}

	private async Task AnalyzeAllOpinionsAsync()
	{
		_isAnalyzing = true;

		List<Task> tasks = [];
		foreach (var opinion in _opinions)
		{
			tasks.Add(AnalyzeOpinionAsync(opinion));
		}
		await Task.WhenAll(tasks);

		_isAnalyzing = false;
	}

	private async Task AnalyzeOpinionAsync(Opinion opinion)
	{
		if (opinion.Review != null)
		{
			opinion.Sentiment = await AzureAiHttpRepository.AnalyzeSentimentAsync(opinion.Review);
			opinion.KeyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(opinion.Review);
		}
	}
}