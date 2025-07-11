using System.Diagnostics;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Services;

public class FileService(IWebHostEnvironment webHostEnv) : IFileService
{
	private readonly string _webRootPath = webHostEnv.WebRootPath;

	public async Task<List<string>> ReadAllLinesAsync(string catalog, string fileName, char trimChar = default)
	{
		var lines = await File.ReadAllLinesAsync(Path.Combine(_webRootPath, "data", catalog, fileName));
		List<string> result = [];
		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i].Trim();

			if (!string.IsNullOrWhiteSpace(line))
			{
				if (trimChar != default)
				{
					line = line.Trim(trimChar);
				}

				result.Add(line);
			}
		}
		return result;
	}

	public async Task<byte[]> ReadImageAsBytesAsync(string fileName) 
		=> await File.ReadAllBytesAsync(Path.Combine(_webRootPath, "images", fileName));

	public async Task<byte[]> ReadInputAsBytesAsync(IBrowserFile file)
	{
		using MemoryStream ms = new();
		// odczytanie zawartości pliku i skopiowanie go do strumienia w pamięci
		// oddatkowo ustawiony maksymalny rozmiar pliku na 10 mb, jak będzie większy to zgłoszony wyjątek
		await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);

		// zwrócenie tablicy bajtów ze strumienia
		return ms.ToArray();
	}

	public string GetBase64String(string contentType, byte[] fileContent)
		=> $"data:{contentType};base64,{Convert.ToBase64String(fileContent)}";

	public string GetFilePath(params string[] paths)
		=> Path.Combine(_webRootPath, Path.Combine(paths));

	// żeby metoda działała, potrzebny plik wwwroot/ffmpeg/ffmpeg.exe
	public async Task<byte[]> ConvertWebmToWavAsync(byte[] webmBytes)
	{
		// tymczasowe zapisanie pliku na dysku w formacie .webm w wwwroot
		var tempWebmPath = Path.Combine(_webRootPath, $"{Guid.NewGuid()}.webm");

		// tymczasowa ścieżka docelowa, czyli gdzie będzie przekonwertowany plik .wav
		var tempWavPath = Path.Combine(_webRootPath, $"{Guid.NewGuid()}.wav");

		// ścieżka do pliku ffmpeg.exe
		var ffmpegPath = Path.Combine(_webRootPath, "ffmpeg", $"ffmpeg.exe");

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