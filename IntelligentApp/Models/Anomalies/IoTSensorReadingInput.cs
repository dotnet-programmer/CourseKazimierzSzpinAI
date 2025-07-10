using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

public class IoTSensorReadingInput
{
	[LoadColumn(0)]
	public string? Timestamp { get; set; }

	[LoadColumn(1)]
	public float Temperature { get; set; }

	[LoadColumn(2)]
	public float Humidity { get; set; }

	[LoadColumn(3)]
	public float Pressure { get; set; }

	[LoadColumn(4)]
	public float Vibration { get; set; }
}