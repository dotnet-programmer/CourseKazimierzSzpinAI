using System.Diagnostics;
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
		var wavFile = await ConvertWebmToWavAsync(_selectedAudio);

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

	// żeby metoda działałą, potrzebny plik wwwroot/ffmpeg/ffmpeg.exe
	private async Task<byte[]> ConvertWebmToWavAsync(byte[] webmBytes)
	{
		// tymczasowe zapisanie pliku na dysku w formacie .webm w wwwroot
		var tempWebmPath = FileService.GetFilePath($"{Guid.NewGuid()}.webm");

		// tymczasowa ścieżka docelowa, czyli gdzie będzie przekonwertowany plik .wav
		var tempWavPath = FileService.GetFilePath($"{Guid.NewGuid()}.wav");

		// ścieżka do pliku ffmpeg.exe
		var ffmpegPath = FileService.GetFilePath("ffmpeg", $"ffmpeg.exe");

		try
		{
			// zapisanie oryginalnego pliku .webm pod tymczasową ścieżką
			await File.WriteAllBytesAsync(tempWebmPath, webmBytes);

			// komenda wywoływana w cmd, czyli konwersja z .webm na .wav i zapisanie pod nową tymczasową ścieżką
			var ffmpegArgs = $"-i \"{tempWebmPath}\" -acodec pcm_s16le -ac 1 -ar 16000 \"{tempWavPath}\"";

			// komendę trzeba wywołać w nowym procesie, dodatkowo przekazanie argumentów w obiekcie ProcessStartInfo
			ProcessStartInfo startInfo = new()
			{
				// nazwa pliku który ma zostać uruchomiony
				FileName = ffmpegPath,

				// argumenty uruchomieniowe, czyli komenda która zostanie wywołana
				Arguments = ffmpegArgs,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			using Process process = new() { StartInfo = startInfo };
			process.Start();

			// do sprawdzenia, czy wystąpiły jakieś błędy podczas uruchomienia procesu
			var stdOutput = await process.StandardOutput.ReadToEndAsync();
			var stdError = await process.StandardError.ReadToEndAsync();

			// czekanie aż zakończy się wykonywanie procesu
			await process.WaitForExitAsync();

			// jeżeli coś poszło nie tak
			if (process.ExitCode != 0)
			{
				Console.WriteLine("ffmpeg error output: " + stdError);
				throw new Exception($"ffmpeg failed with exit code {process.ExitCode}");
			}

			// sprawdzenie czy został utworzony nowy plik .wav
			if (!File.Exists(tempWavPath))
			{
				throw new FileNotFoundException($"ffmpeg output not found: {tempWavPath}");
			}

			// zwrócenie nowego pliku .wav jako tablica bajtów
			return await File.ReadAllBytesAsync(tempWavPath);
		}
		// usunięcie plików tymczasowych
		finally
		{
			if (File.Exists(tempWebmPath))
			{
				File.Delete(tempWebmPath);
			}

			if (File.Exists(tempWavPath))
			{
				File.Delete(tempWavPath);
			}
		}
	}
}