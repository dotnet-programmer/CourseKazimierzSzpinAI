namespace IntelligentApp.HttpRepository.Interfaces;

public interface IAzureSpeechHttpRepository
{
	Task<byte[]?> GetVoiceAsync(string text, string language = "pl-PL", string voice = "pl-PL-MarekNeural", double speed = 100);
	Task<string?> GetTextAsync(byte[] audioData);
}