using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class AnalysisInput
{
	[JsonPropertyName("documents")]
	public List<AnalysisDocument> Documents { get; set; }
}