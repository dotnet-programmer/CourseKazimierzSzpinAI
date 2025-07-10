using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

// klasa z Outputem 
public class TransactionsPrediction
{
	// informacje o numerze klastra
	[ColumnName("PredictedLabel")]
	public uint PredictedClusterId { get; set; }

	// odległość do każdego centroidu
	// w przykładzie jest tylko 1 klaster więc na 0 indeksie będzie odległość od centroidu
	[ColumnName("Score")]
	public float[]? Distances { get; set; }
}