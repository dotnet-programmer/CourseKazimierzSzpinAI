using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IntelligentApp.Components.Pages.AzureSpeech;

public partial class VoiceAssistant
{
	private bool _isLoading = false;
	private bool _isRecording = false;
	private byte[]? _selectedAudio;
	private string? _transcript;
	private string? _audioDataUrl;
	private string? _openAIResponse;

	[Inject]
	protected IJSRuntime JS { get; set; } = default!;

	[Inject]
	protected IAzureSpeechHttpRepository AzureSpeechHttpRepository { get; set; } = default!;

	[Inject]
	protected IOpenAiHttpRepository OpenAiHttpRepository { get; set; } = default!;

	[Inject]
	protected IFileService FileService { get; set; } = default!;

	private async Task StartRecordingAsync()
	{
		_isRecording = true;
		_selectedAudio = null;
		_transcript = null;
		_openAIResponse = null;
		_audioDataUrl = null;

		await JS.InvokeVoidAsync("audioRecorder.startRecording");
	}

	private async Task StopRecordingAsync()
	{
		_isRecording = false;

		var base64 = await JS.InvokeAsync<string>("audioRecorder.stopRecording");

		if (!string.IsNullOrWhiteSpace(base64))
		{
			_selectedAudio = Convert.FromBase64String(base64);
		}
	}

	private async Task ProcessQuestionAsync()
	{
		if (_selectedAudio == null)
		{
			return;
		}

		_isLoading = true;
		_transcript = null;
		_openAIResponse = null;
		_audioDataUrl = null;

		// przed wysłaniem pliku audio trzeba przekonwertować go np. do formatu WAV
		var wavFile = await FileService.ConvertWebmToWavAsync(_selectedAudio);
		
		_transcript = await AzureSpeechHttpRepository.GetTextAsync(wavFile);

		if (_transcript == null)
		{
			_isLoading = false;
			return;
		}

		_openAIResponse = await OpenAiHttpRepository.AskOpenAiAsync(_transcript);

		if (_openAIResponse == null)
		{
			_isLoading = false;
			return;
		}

		var ttsAudioData = await AzureSpeechHttpRepository.GetVoiceAsync(_openAIResponse);
		if (ttsAudioData != null)
		{
			_audioDataUrl = FileService.GetBase64String("audio/wav", ttsAudioData);
		}

		_isLoading = false;
	}
}