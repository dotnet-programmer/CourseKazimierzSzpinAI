using IntelligentApp.HttpRepository.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.AzureAi_OpenAi;

public partial class OpenAiVsAzureAi
{
	private List<string> _keyPhrases = [];
	private string _baseText = string.Empty;
	private string _generatedText = string.Empty;
	private bool _isLoading = false;

	[Inject]
	protected IOpenAiHttpRepository OpenAiHttpRepository { get; set; } = default!;

	[Inject]
	protected IAzureAiHttpRepository AzureAiHttpRepository { get; set; } = default!;

	private async Task ProcessTextAsync()
	{
		_isLoading = true;

		_generatedText = await OpenAiHttpRepository.AskOpenAiAsync($"Przeczytaj poniższy tekst i wygeneruj krótkie streszczenie:\n\n{_baseText}.");
		_keyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(_baseText);

		_isLoading = false;
	}

	private MarkupString HighlightKeyPhrases(string text, List<string> phrases)
	{
		var highlighted = text;
		foreach (var phrase in phrases)
		{
			if (!string.IsNullOrWhiteSpace(phrase))
			{
				highlighted = highlighted.Replace(phrase, $"<span style=\"background-color:yellow\">{phrase}</span>", StringComparison.OrdinalIgnoreCase);
			}
		}
		return (MarkupString)highlighted;
	}
}