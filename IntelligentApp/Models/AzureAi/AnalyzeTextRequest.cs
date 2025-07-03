using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalyzeTextRequest
{
	[JsonPropertyName("kind")]
	public string Kind { get; set; }

	[JsonPropertyName("analysisInput")]
	public AnalysisInput AnalysisInput { get; set; }

	[JsonPropertyName("parameters")]
	public Dictionary<string, object> Parameters { get; set; }
}