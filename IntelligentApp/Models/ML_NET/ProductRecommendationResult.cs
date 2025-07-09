namespace IntelligentApp.Models.ML_NET;

// pomocnicza klasa do wyświetlania wyników predykcji
public class ProductRecommendationResult
{
	public int ProductId { get; set; }
	public float PredictedScore { get; set; }
}