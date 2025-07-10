using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Services;

public class FileReaderService(IWebHostEnvironment webHostEnv) : IFileReader
{
	public async Task<List<string>> ReadAllLinesAsync(string catalog, string fileName, char trimChar = default)
	{
		var lines = await File.ReadAllLinesAsync(Path.Combine(webHostEnv.WebRootPath, "data", catalog, fileName));
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
		=> await File.ReadAllBytesAsync(Path.Combine(webHostEnv.WebRootPath, "images", fileName));

	public async Task<byte[]> ReadInputAsBytesAsync(IBrowserFile file)
	{
		using MemoryStream ms = new();
		// odczytanie zawartości pliku i skopiowanie go do strumienia w pamięci
		// oddatkowo ustawiony maksymalny rozmiar pliku na 10 mb, jak będzie większy to zgłoszony wyjątek
		await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);

		// zwrócenie tablicy bajtów ze strumienia
		return ms.ToArray();
	}
}