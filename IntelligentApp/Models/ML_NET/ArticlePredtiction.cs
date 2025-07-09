using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ArticlePredtiction
{
	[ColumnName("Score")]
	public float PredictedViews { get; set; }
}