using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.OpenAi;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.AzureAi_OpenAi;

public partial class AdvancedTextGeneration
{
	private OpenAiRequestKind _requestKind = OpenAiRequestKind.Normal;
	private string _userPrompt = string.Empty;
	private string _baseText = string.Empty;
	private string _generatedText = string.Empty;
	private bool _isLoading = false;

	[Inject]
	protected IOpenAiHttpRepository OpenAiHttpRepository { get; set; } = default!;

	private async Task GenerateTextAsync()
	{
		_isLoading = true;
		_generatedText = string.Empty;
		_generatedText = await OpenAiHttpRepository.AskOpenAiAsync(PreparePrompt());
		_isLoading = false;
	}

	private string PreparePrompt() 
		=> _requestKind switch
		{
			OpenAiRequestKind.Normal => _userPrompt,
			OpenAiRequestKind.Summary => $"Przeczytaj poniższy tekst i wygeneruj krótkie streszczenie:\n\n{_baseText}.",
			OpenAiRequestKind.QA => $"Oto tekst źródłowy:\n\n{_baseText}\n\nNa podstawie powyższego tekstu, odpowiedz na pytanie:\n\n{_userPrompt}.",
			_ => string.Empty
		};
}