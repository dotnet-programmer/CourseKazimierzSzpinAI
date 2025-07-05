using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class OpenAIGenerate
{
	private List<string> _availablePrompts = [];
	private bool _isLoading = false;
	private string _userPrompt = string.Empty;
	private string _generatedText = string.Empty;

	[Inject]
	public IOpenAiHttpRepository OpenAiHttpRepository { get; set; }

	[Inject]
	public IFileReader FileReader { get; set; }

	protected override async Task OnInitializedAsync()
		=> _availablePrompts = await FileReader.ReadAllLinesAsync("prompts.csv", '"');

	private void OnPromptSelected(ChangeEventArgs e)
	{
		var selected = e.Value?.ToString();

		if (!string.IsNullOrWhiteSpace(selected))
		{
			_userPrompt = selected;
		}
	}

	private async Task GenerateTextAsync()
	{
		if (string.IsNullOrWhiteSpace(_userPrompt))
		{
			return;
		}

		_isLoading = true;
		_generatedText = string.Empty;
		_generatedText = await OpenAiHttpRepository.AskOpenAiAsync(_userPrompt);
		_isLoading = false;
	}
}