namespace IntelligentApp.Models.Recommendations;

// klasa pomocnicza która przyporządkuje każdy artykuł z predykcją
public class ArticleRecommendationResult
{
	public int ArticleId { get; set; }
	public float PredictedScore { get; set; }
}