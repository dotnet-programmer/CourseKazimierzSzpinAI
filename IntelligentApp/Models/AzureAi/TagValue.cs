using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class TagValue
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("confidence")]
	public float Confidence { get; set; }
}