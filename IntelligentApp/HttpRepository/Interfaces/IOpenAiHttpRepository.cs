namespace IntelligentApp.HttpRepository.Interfaces;

public interface IOpenAiHttpRepository
{
	Task<string> AskOpenAiAsync(string prompt, string aiModel = "gpt-4", int maxTokens = 100, string endpoint = "");
}