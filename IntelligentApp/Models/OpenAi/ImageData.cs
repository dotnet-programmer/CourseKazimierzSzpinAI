using System.Text.Json.Serialization;

namespace IntelligentApp.Models.OpenAi;

public class ImageData
{
	[JsonPropertyName("url")]
	public string? Url { get; set; }
}