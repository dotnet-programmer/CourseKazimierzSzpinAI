namespace IntelligentApp.Models.Recommendations;

// pomocnicza klasa do wyświetlania wyników predykcji
public class ProductRecommendationResult
{
	public int ProductId { get; set; }
	public float PredictedScore { get; set; }
}