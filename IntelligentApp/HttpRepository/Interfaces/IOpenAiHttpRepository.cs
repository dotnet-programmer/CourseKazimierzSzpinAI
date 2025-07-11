using IntelligentApp.Models.ContentAssistant;

namespace IntelligentApp.HttpRepository.Interfaces;

public interface IOpenAiHttpRepository
{
	Task<string> AskOpenAiAsync(string prompt, string aiModel = "gpt-4", int maxTokens = 100, string endpoint = "");
	Task<string> AskOpenAiWithHistoryAsync(List<ChatMessage> chatMessages, string aiModel = "gpt-4", int maxTokens = 100, string endpoint = "");
	Task<string> GenerateImageAsync(string prompt, int count = 1, string size = "256x256");
}