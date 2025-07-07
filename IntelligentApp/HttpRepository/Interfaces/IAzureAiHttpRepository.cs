using IntelligentApp.Models.AzureAi;

namespace IntelligentApp.HttpRepository.Interfaces;

public interface IAzureAiHttpRepository
{
	Task<string> AnalyzeSentimentAsync(string text);
	Task<List<string>> ExtractKeyPhrasesAsync(string text);
	Task<ImageAnalysisResponse> GetImageInfoAsync(byte[] image);
}