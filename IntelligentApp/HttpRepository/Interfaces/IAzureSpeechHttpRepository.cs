namespace IntelligentApp.HttpRepository.Interfaces;

public interface IAzureSpeechHttpRepository
{
	Task<byte[]?> GetVoiceAsync(string text);
	Task<string?> GetTextAsync(byte[] audioData);
}