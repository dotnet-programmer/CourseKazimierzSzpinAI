using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureVision;

public class TagValue
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("confidence")]
	public float Confidence { get; set; }
}