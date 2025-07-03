using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalyzeTextDocument
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("sentiment")]
	public string Sentiment { get; set; }

	[JsonPropertyName("confidenceScores")]
	public ConfidenceScores ConfidenceScores { get; set; }

	[JsonPropertyName("keyPhrases")]
	public List<string> KeyPhrases { get; set; }
}