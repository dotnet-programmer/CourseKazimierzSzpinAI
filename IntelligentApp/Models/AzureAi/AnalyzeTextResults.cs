using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalyzeTextResults
{
	[JsonPropertyName("documents")]
	public List<AnalyzeTextDocument> Documents { get; set; }

	[JsonPropertyName("errors")]
	public List<AnalyzeTextError> Errors { get; set; }

	[JsonPropertyName("modelVersion")]
	public string ModelVersion { get; set; }
}