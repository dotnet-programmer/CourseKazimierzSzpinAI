namespace IntelligentApp.Models.Recommendations;

// przypisanie wektora cech do konkretnego filmu
public class MovieVector
{
	public float MovieId { get; set; }
	public float[]? Features { get; set; }
}