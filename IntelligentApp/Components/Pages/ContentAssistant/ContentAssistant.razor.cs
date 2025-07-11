using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.JSInterop;

namespace IntelligentApp.Components.Pages.ContentAssistant;

public partial class ContentAssistant(IJSRuntime JS, IFileService fileService, IAzureSpeechHttpRepository azureSpeech)
{
	private bool _isRecording = false;
	private string? _userProductDesc;
	private byte[]? _audio;

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
}