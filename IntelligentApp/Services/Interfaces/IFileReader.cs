namespace IntelligentApp.Services.Interfaces;

public interface IFileReader
{
	Task<List<string>> ReadAllLinesAsync(string fileName, char trimChar = default);
	Task<byte[]> ReadImageAsBytes(string fileName);
}