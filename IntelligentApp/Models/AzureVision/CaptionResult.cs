using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureVision;

public class CaptionResult
{
	[JsonPropertyName("text")]
	public string Text { get; set; }

	[JsonPropertyName("confidence")]
	public float Confidence { get; set; }
}