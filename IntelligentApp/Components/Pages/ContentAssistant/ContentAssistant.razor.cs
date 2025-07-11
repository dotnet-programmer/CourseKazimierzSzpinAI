using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.ContentAssistant;
using IntelligentApp.Services.Interfaces;
using Microsoft.JSInterop;

namespace IntelligentApp.Components.Pages.ContentAssistant;

public partial class ContentAssistant(IJSRuntime JS, IFileService fileService, IAzureSpeechHttpRepository azureSpeech, IOpenAiHttpRepository openAi)
{
	private bool _isLoading = false;
	private bool _isRecording = false;
	private string? _userProductDesc;
	private byte[]? _audio;
	private string? _generatedName;
	private string? _generatedDescription;
	private string? _notes;
	private List<ChatMessage> _nameMessages = [];
	private List<ChatMessage> _descriptionMessages = [];
	private string? _imageDataUrl;

	private async Task StartRecordingAsync()
	{
		_isRecording = true;
		_userProductDesc = null;
		_audio = null;
		await JS.InvokeVoidAsync("audioRecorder.startRecording");
	}

	private async Task StopRecordingAsync()
	{
		var base64 = await JS.InvokeAsync<string>("audioRecorder.stopRecording");

		if (!string.IsNullOrWhiteSpace(base64))
		{
			_audio = Convert.FromBase64String(base64);
		}

		if (_audio == null)
		{
			return;
		}

		var wavFile = await fileService.ConvertWebmToWavAsync(_audio);
		_userProductDesc = await azureSpeech.GetTextAsync(wavFile);
		_isRecording = false;
	}

	private async Task GenerateAsync()
	{
		if (string.IsNullOrWhiteSpace(_userProductDesc))
		{
			return;
		}

		_isLoading = true;

		await GenerateNameAsync();
		await GenerateDescriptionAsync();
		await GenerateImageAsync();

		_isLoading = false;
	}

	private async Task GenerateNameAsync()
	{
		var prompt = $@"Wygeneruj krótką, chwytliwą nazwę produktu na podstawie poniższych informacji: {_userProductDesc} Użyj języka polskiego. Podaj tylko samą nazwę, bez dodatkowego wyjaśnienia.";

		// dodanie prompta użytkownika do historii rozmowy
		_nameMessages.Add(new ChatMessage { Role = "user", Content = prompt });
		_generatedName = await openAi.AskOpenAiWithHistoryAsync(_nameMessages);
		// dodanie odpowiedzi OpenAi do historii rozmowy
		_nameMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedName });
	}

	private async Task GenerateDescriptionAsync()
	{
		var prompt = $@"Na podstawie tych informacji o produkcie: {_userProductDesc} Stwórz angażujący, marketingowy opis produktu w języku polskim. Napisz w taki sposób, by był atrakcyjny dla potencjalnego klienta. Opis ma zawierać około 100 znaków.";

		_descriptionMessages.Add(new ChatMessage { Role = "user", Content = prompt });
		_generatedDescription = await openAi.AskOpenAiWithHistoryAsync(_descriptionMessages);
		_descriptionMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedDescription });
	}

	private async Task ChangeNameAsync()
	{
		_nameMessages.Add(new ChatMessage { Role = "user", Content = _notes });
		_generatedName = await openAi.AskOpenAiWithHistoryAsync(_nameMessages);
		_nameMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedName });
	}

	private async Task ChangeDescriptionAsync()
	{
		_descriptionMessages.Add(new ChatMessage { Role = "user", Content = _notes });
		_generatedDescription = await openAi.AskOpenAiWithHistoryAsync(_descriptionMessages);
		_descriptionMessages.Add(new ChatMessage { Role = "assistant", Content = _generatedDescription });
	}

	private async Task GenerateImageAsync()
	{
		var dallePrompt = $"Stwórz mi prompta, który mogę wysłać do DALLE, tak żeby zostało wygenerowane ładne zdjęcie mojego produktu o nazwie: {_generatedName}. Opis produktu: {_generatedDescription}. To zdjęcie będę chciał ustawić jako miniaturka w moim sklepie internetowym.";
		_imageDataUrl = await openAi.GenerateImageAsync(dallePrompt);
	}
}