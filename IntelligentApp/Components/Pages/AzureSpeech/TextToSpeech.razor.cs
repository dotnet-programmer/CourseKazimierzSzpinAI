using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages.AzureSpeech;

public partial class TextToSpeech
{
	private readonly List<string> _languages = [];
	private readonly List<string> _voices = [];
	private string _selectedLanguage = "pl-PL";
	private string _selectedVoice = "pl-PL-MarekNeural";
	private double _speed = 100;
	private string _textForTTS = "Witaj na naszym kursie AI dla .NET!";
	private string? _audioDataUrl;

	[Inject]
	protected IAzureSpeechHttpRepository AzureSpeechHttpRepository { get; set; } = default!;

	[Inject]
	protected IFileService FileService { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		var lines = await FileService.ReadAllLinesAsync("azure-speech", "voices.csv");

		for (var i = 0; i < lines.Count; i++)
		{
			var splitLine = lines[i].Replace("\"", "").Split(',');

			if(!_languages.Contains(splitLine[0]))
			{
				_languages.Add(splitLine[0]);
			}

			_voices.Add(splitLine[1]);
		}

		if (_languages.Count == 0)
		{
			_languages.Add("pl-PL");
		}

		if (_voices.Count == 0)
		{
			_voices.Add("pl-PL-MarekNeural");
		}
	}

	private async Task SynthesizeSpeechAsync()
	{
		_audioDataUrl = null;

		try
		{
			var audioData = await AzureSpeechHttpRepository.GetVoiceAsync(_textForTTS, _selectedLanguage, _selectedVoice, _speed);

			if (audioData == null)
			{
				return;
			}

			_audioDataUrl = FileService.GetBase64String("audio/wav", audioData);
		}
		catch (Exception)
		{
			//logowanie
			throw;
		}
	}
}