namespace IntelligentApp.Models.ContentAssistant;

// połączenie produktów z tabelą features, czyli wektorem cech
public class ProductVector
{
	public float ProductId { get; set; }
	public float[]? Features { get; set; }
}