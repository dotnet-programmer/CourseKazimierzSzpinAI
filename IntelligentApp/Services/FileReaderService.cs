using IntelligentApp.Services.Interfaces;

namespace IntelligentApp.Services;

public class FileReaderService(IWebHostEnvironment webHostEnv) : IFileReader
{
	public async Task<List<string>> ReadAllLinesAsync(string fileName, char trimChar = default)
	{
		var lines = await File.ReadAllLinesAsync(Path.Combine(webHostEnv.WebRootPath, "data", fileName));
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

	public async Task<byte[]> ReadImageAsBytes(string fileName) 
		=> await File.ReadAllBytesAsync(Path.Combine(webHostEnv.WebRootPath, "images", fileName));
}