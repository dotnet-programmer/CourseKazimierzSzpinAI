using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ConfidenceScores
{
	[JsonPropertyName("positive")]
	public double Positive { get; set; }

	[JsonPropertyName("neutral")]
	public double Neutral { get; set; }

	[JsonPropertyName("negative")]
	public double Negative { get; set; }
}