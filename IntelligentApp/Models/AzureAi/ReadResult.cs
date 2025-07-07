using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ReadResult
{
	[JsonPropertyName("blocks")]
	public List<ReadBlock> Blocks { get; set; }
}