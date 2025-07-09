using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class CustomerClusterPrediction
{
	[ColumnName("PredictedLabel")]
	public uint ClusterId;

	[ColumnName("Score")]
	public float[]? Distances;
}