using IntelligentApp.HttpRepository.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class TextAnalysis
{
	private bool _isLoading = false;
	private string _userText = string.Empty;
	private string _sentimentResult = string.Empty;
	private List<string> _keyPhrases = [];

	[Inject]
	protected IAzureAiHttpRepository AzureAiHttpRepository { get; set; } = default!;

	private async Task AnalyzeTextAsync()
	{
		if (string.IsNullOrWhiteSpace(_userText))
		{
			return;
		}

		_isLoading = true;
		
		_sentimentResult = string.Empty;
		_sentimentResult = await AzureAiHttpRepository.AnalyzeSentimentAsync(_userText);
		
		_keyPhrases.Clear();
		_keyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(_userText);
		
		_isLoading = false;
	}
}