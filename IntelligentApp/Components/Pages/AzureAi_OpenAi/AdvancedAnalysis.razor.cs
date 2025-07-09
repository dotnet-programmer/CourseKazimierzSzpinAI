using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.OpenAi;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.AzureAi_OpenAi;

public partial class AdvancedAnalysis
{
	private OpenAiRequestKind _requestKind = OpenAiRequestKind.Summary;
	private List<string> _keyPhrases = [];
	private string _baseText = string.Empty;
	private string _userPrompt = string.Empty;
	private string _generatedText = string.Empty;
	private string _sentimentResult = string.Empty;
	private bool _isLoading = false;

	[Inject]
	protected IOpenAiHttpRepository OpenAiHttpRepository { get; set; } = default!;

	[Inject]
	protected IAzureAiHttpRepository AzureAiHttpRepository { get; set; } = default!;

	private async Task ProcessTextAsync()
	{
		_isLoading = true;

		_generatedText = await OpenAiHttpRepository.AskOpenAiAsync(PreparePrompt());
		_sentimentResult = await AzureAiHttpRepository.AnalyzeSentimentAsync(_generatedText);
		_keyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(_generatedText);

		_isLoading = false;
	}

	private string PreparePrompt()
		=> _requestKind switch
		{
			OpenAiRequestKind.Summary => $"Przeczytaj poniższy tekst i wygeneruj krótkie streszczenie:\n\n{_baseText}.",
			OpenAiRequestKind.QA => $"Oto tekst źródłowy:\n\n{_baseText}\n\nNa podstawie powyższego tekstu, odpowiedz na pytanie:\n\n{_userPrompt}.",
			_ => string.Empty
		};
}