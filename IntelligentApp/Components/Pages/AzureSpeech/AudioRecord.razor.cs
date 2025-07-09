using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IntelligentApp.Components.Pages.AzureSpeech;

public partial class AudioRecord
{
	private bool _isRecording = false;

	// adres URL do nagrania w formacie Base64
	private string _audioDataUrl = string.Empty;

	[Inject]
	protected IJSRuntime JS { get; set; } = default!;

	private async Task StartRecordingAsync()
	{
		try
		{
			_isRecording = true;
			await JS.InvokeVoidAsync("audioRecorder.startRecording");
		}
		catch (Exception)
		{
			//logowanie
			throw;
		}
	}

	private async Task StopRecordingAsync()
	{
		try
		{
			_isRecording = false;
			// wywołanie funkcji JS ze wskazaniem zwracanego typu (string)
			var base64 = await JS.InvokeAsync<string>("audioRecorder.stopRecording");

			if (!string.IsNullOrWhiteSpace(base64))
			{
				_audioDataUrl = $"data:audio/wav;base64,{base64}";
			}
		}
		catch (Exception)
		{
			//logowanie
			throw;
		}
	}
}