namespace IntelligentApp.Models.ML_NET;

// do wyświetlenia podobnego filmu
// dla danego filmu Movie będzie wyświetlała podobieństwo Similarity
public class SimilarMovie
{
	public MovieData? Movie { get; set; }
	public float Similarity { get; set; }
}