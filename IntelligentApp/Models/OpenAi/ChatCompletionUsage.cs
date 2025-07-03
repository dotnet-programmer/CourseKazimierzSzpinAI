namespace IntelligentApp.Models.OpenAi;

public class ChatCompletionUsage
{
	public int PromptTokens { get; set; }
	public int CompletionTokens { get; set; }
	public int TotalTokens { get; set; }
}