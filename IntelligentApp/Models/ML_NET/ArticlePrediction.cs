using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ArticlePrediction
{
	[ColumnName("Score")]
	public float PredictedViews { get; set; }
}