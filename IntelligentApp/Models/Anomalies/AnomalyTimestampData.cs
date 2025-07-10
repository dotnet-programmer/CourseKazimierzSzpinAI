using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

public class AnomalyTimestampData
{
	// data i czas logu
	[LoadColumn(0)]
	public string? Timestamp { get; set; }

	// wartość numeryczna do analizy
	[LoadColumn(1)]
	public float Value { get; set; }
}