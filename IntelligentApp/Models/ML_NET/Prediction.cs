using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

// tutaj będą wyniki predykcji 
public class Prediction
{
	// w strumieniu danych ML.NET kolumna PredictedLabel ma trafić do właściwości PredictedLabel typu string
	[ColumnName("PredictedLabel")]
	public string? PredictedLabel { get; set; }
}