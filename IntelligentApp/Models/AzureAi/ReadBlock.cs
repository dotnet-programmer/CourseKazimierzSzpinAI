using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ReadBlock
{
	[JsonPropertyName("lines")]
	public List<ReadLine> Lines { get; set; }
}