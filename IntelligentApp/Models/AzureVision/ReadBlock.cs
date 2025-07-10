using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureVision;

public class ReadBlock
{
	[JsonPropertyName("lines")]
	public List<ReadLine> Lines { get; set; }
}