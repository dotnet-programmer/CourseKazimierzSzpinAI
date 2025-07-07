using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ReadWord
{
	[JsonPropertyName("text")]
	public string Text { get; set; }

	[JsonPropertyName("boundingPolygon")]
	public List<BoundingPoint> BoundingPolygon { get; set; }

	[JsonPropertyName("confidence")]
	public float Confidence { get; set; }
}