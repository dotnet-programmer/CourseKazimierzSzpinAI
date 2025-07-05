using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class AIPlayground
{
	private List<string> _availablePrompts = [];

	private bool _isLoading = false;
	private string _userText = string.Empty;
	private string _generatedText = string.Empty;
	private string _sentimentResult = string.Empty;
	private List<string> _keyPhrases = [];

	[Inject]
	public IFileReader FileReader { get; set; }

	[Inject]
	public IOpenAiHttpRepository OpenAiHttpRepository { get; set; }

	[Inject]
	public IAzureAiHttpRepository AzureAiHttpRepository { get; set; }

	protected override async Task OnInitializedAsync()
		=> _availablePrompts = await FileReader.ReadAllLinesAsync("prompts.csv", '"');

	private void OnPromptSelected(ChangeEventArgs e)
	{
		var selected = e.Value?.ToString();

		if (!string.IsNullOrWhiteSpace(selected))
		{
			_userText = selected;
		}
	}

	private async Task GenerateResponseAsync()
	{
		if (string.IsNullOrWhiteSpace(_userText))
		{
			return;
		}
		_isLoading = true;
		ClearInputs();
		_generatedText = await OpenAiHttpRepository.AskOpenAiAsync(_userText);
		_sentimentResult = await AzureAiHttpRepository.AnalyzeSentimentAsync(_generatedText);
		_keyPhrases = await AzureAiHttpRepository.ExtractKeyPhrasesAsync(_generatedText);
		_isLoading = false;
	}

	private void ClearInputs()
	{
		_generatedText = _sentimentResult = string.Empty;
		_keyPhrases.Clear();
	}
}