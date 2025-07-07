using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class Metadata
{
	[JsonPropertyName("width")]
	public int Width { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }
}