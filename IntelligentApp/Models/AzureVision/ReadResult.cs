using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureVision;

public class ReadResult
{
	[JsonPropertyName("blocks")]
	public List<ReadBlock> Blocks { get; set; }
}