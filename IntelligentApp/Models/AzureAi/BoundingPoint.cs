using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class BoundingPoint
{
	[JsonPropertyName("x")]
	public int X { get; set; }

	[JsonPropertyName("y")]
	public int Y { get; set; }
}