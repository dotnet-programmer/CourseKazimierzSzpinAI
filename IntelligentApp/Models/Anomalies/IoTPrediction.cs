using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

public class IoTPrediction
{
	// tu będzie informacja czy wartość jest anomalią
	[ColumnName("PredictedLabel")]
	public bool PredictedLabel { get; set; }

	// wartość numeryczna anomalii, im wyższa wartość, tym bardziej nietypowa wartość
	public float Score { get; set; }
}