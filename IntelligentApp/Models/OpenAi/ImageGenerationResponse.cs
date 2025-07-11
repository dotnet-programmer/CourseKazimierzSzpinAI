using System.Text.Json.Serialization;

namespace IntelligentApp.Models.OpenAi;

public class ImageGenerationResponse
{
	[JsonPropertyName("created")]
	public long Created { get; set; }

	[JsonPropertyName("data")]
	public List<ImageData>? Data { get; set; }
}