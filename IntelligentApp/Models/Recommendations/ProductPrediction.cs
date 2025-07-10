using Microsoft.ML.Data;

namespace IntelligentApp.Models.Recommendations;

public class ProductPrediction
{
	[ColumnName("Score")]
	public float Score { get; set; }
}