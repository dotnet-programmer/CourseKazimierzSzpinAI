using IntelligentApp.Models.AzureVision;

namespace IntelligentApp.HttpRepository.Interfaces;

public interface IAzureVisionHttpRepository
{
	Task<ImageAnalysisResponse?> GetImageInfoAsync(byte[] image);
}