using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ProductPrediction
{
	[ColumnName("Score")]
	public float Score { get; set; }
}