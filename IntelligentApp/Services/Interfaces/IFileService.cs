using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Services.Interfaces;

public interface IFileService
{
	Task<List<string>> ReadAllLinesAsync(string catalog, string fileName, char trimChar = default);
	Task<byte[]> ReadImageAsBytesAsync(string fileName);
	Task<byte[]> ReadInputAsBytesAsync(IBrowserFile file);
	string GetBase64String(string contentType, byte[] selectedFileContent);
	string GetFilePath(params string[] paths);
}