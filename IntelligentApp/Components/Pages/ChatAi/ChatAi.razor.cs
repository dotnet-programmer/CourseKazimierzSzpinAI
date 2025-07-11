using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.OpenAi;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.ChatAi;

public partial class ChatAi(IFileService fileService, IOpenAiHttpRepository openAi)
{
	private readonly List<ChatCompletionMessage> _messages = [];
	private readonly List<string> _availablePrompts = [];

	private string? _currentPrompt;

	protected override async Task OnInitializedAsync()
	{
		_messages.Add(new ChatCompletionMessage { Role = "System", Content = "Witaj, zadaj mi pytanie!" });
		var lines = await fileService.ReadAllLinesAsync("openai", "prompts.csv", '"');
		_availablePrompts.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
	}

	private void OnPromptSelected(ChangeEventArgs e)
	{
		var selected = e.Value?.ToString();

		if (!string.IsNullOrWhiteSpace(selected))
		{
			_currentPrompt = selected;
		}
	}

	private async Task SendPromptAsync()
	{
		if (string.IsNullOrWhiteSpace(_currentPrompt))
		{
			return;
		}

		_messages.Add(new ChatCompletionMessage { Role = "User", Content = _currentPrompt });
		var answer = await openAi.AskOpenAiAsync(_currentPrompt);
		_messages.Add(new ChatCompletionMessage { Role = "AI", Content = answer });
		await fileService.LogConversationAsync(_currentPrompt, answer);
		_currentPrompt = string.Empty;
	}
}