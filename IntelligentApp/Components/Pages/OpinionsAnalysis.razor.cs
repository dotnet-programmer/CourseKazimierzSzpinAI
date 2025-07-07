using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class OpinionsAnalysis
{
	private readonly List<Opinion> _opinions = [];
	
	private bool _isLoading = false;
	private bool _isAnalyzing = false;

	[Inject]
	public IFileReader FileReader { get; set; }

	[Inject]
	public IAzureAiHttpRepository AzureAiHttpRepository { get; set; }

	private async Task LoadOpinionsAsync()
	{
		_isLoading = true;

		_opinions.Clear();
		var lines = await FileReader.ReadAllLinesAsync("opinions.csv", '"');
		for (int i = 1; i < lines.Count; i++)
		{
			_opinions.Add(new() { Review = lines[i] });
		}
		//lines.RemoveAt(0);
		//lines.ForEach(x => _opinions.Add(new() { Review = x }));

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

		//for (int i = 0; i < _opinions.Count; i++)
		//{
		//	await AnalyzeOpinionAsync(_opinions[i]);
		//}

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
		opinion.Sentiment = await AzureAiHttpRepository.AnalyzeSentimentAsync(opinion.Review);
		opinion.KeyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(opinion.Review);
	}
}