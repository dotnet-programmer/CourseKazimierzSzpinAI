using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

// klasa predykcji gdzie będzie wynik prawdopodobieństwa
public class ArticlePrediction
{
	[ColumnName("Score")]
	public float Score { get; set; }
}