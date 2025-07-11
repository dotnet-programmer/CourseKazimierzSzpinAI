namespace IntelligentApp.Models.ContentAssistant;

// do wyświetlenia podobnego produktu
public class SimilarProduct
{
	public ProductData? Product { get; set; }
	public float Similarity { get; set; }
}