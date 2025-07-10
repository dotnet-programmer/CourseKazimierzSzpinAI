namespace IntelligentApp.Models.Anomalies;

// klasa pomocnicza do wyśwetlania danych
public class IoTSensorReading : IoTSensorReadingInput
{
	//public string? Timestamp { get; set; }
	//public float Temperature { get; set; }
	//public float Humidity { get; set; }
	//public float Pressure { get; set; }
	//public float Vibration { get; set; }

	public IoTPrediction Prediction { get; set; }
}