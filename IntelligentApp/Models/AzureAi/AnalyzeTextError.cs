using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalyzeTextError
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("error")]
	public ErrorDetails Error { get; set; }
}