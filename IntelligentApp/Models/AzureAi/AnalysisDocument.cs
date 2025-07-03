using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalysisDocument
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("language")]
	public string Language { get; set; }

	[JsonPropertyName("text")]
	public string Text { get; set; }
}