using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class TextToSpeech
{
	private string _textForTTS = "Witaj na naszym kursie AI dla .NET!";
	private string? _audioDataUrl = string.Empty;

	[Inject]
	protected IAzureSpeechHttpRepository AzureSpeechHttpRepository { get; set; } = default!;

	[Inject]
	protected IFileReader FileReader { get; set; } = default!;

	private async Task SynthesizeSpeechAsync()
	{
		_audioDataUrl = null;

		try
		{
			var audioData = await AzureSpeechHttpRepository.GetVoiceAsync(_textForTTS);

			if (audioData == null)
			{
				return;
			}

			var base64 = Convert.ToBase64String(audioData);

			if (!string.IsNullOrWhiteSpace(base64))
			{
				_audioDataUrl = $"data:audio/wav;base64,{base64}";
			}
		}
		catch (Exception ex)
		{
			//logowanie
			throw;
		}
	}
}