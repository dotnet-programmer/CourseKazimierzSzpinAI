using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ReadLine
{
	[JsonPropertyName("text")]
	public string Text { get; set; }

	[JsonPropertyName("boundingPolygon")]
	public List<BoundingPoint> BoundingPolygon { get; set; }

	[JsonPropertyName("words")]
	public List<ReadWord> Words { get; set; }
}