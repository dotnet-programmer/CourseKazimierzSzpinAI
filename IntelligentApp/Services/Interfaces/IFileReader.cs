using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Services.Interfaces;

public interface IFileReader
{
	Task<List<string>> ReadAllLinesAsync(string fileName, char trimChar = default);
	Task<byte[]> ReadImageAsBytesAsync(string fileName);
	Task<byte[]> ReadInputAsBytesAsync(IBrowserFile file);
}