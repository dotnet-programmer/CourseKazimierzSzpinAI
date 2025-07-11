using IntelligentApp.HttpRepository.Interfaces;
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

		_isLoading = false;
	}

	private async Task GenerateNameAsync()
	{
		var prompt = $@"
Wygeneruj krótką, chwytliwą nazwę produktu na podstawie poniższych informacji:
{_userProductDesc}
Użyj języka polskiego. Podaj tylko samą nazwę, bez dodatkowego wyjaśnienia.";

		_generatedName = await openAi.AskOpenAiAsync(prompt);
	}

	private async Task GenerateDescriptionAsync()
	{
		var prompt = $@"
Na podstawie tych informacji o produkcie:
{_userProductDesc}
Stwórz angażujący, marketingowy opis produktu w języku polskim.
Napisz w taki sposób, by był atrakcyjny dla potencjalnego klienta.
Opis ma zawierać około 100 znaków.";

		_generatedDescription = await openAi.AskOpenAiAsync(prompt);
	}
}