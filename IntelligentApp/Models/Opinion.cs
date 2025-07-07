namespace IntelligentApp.Models;

public class Opinion
{
	public string Review { get; set; }
	public string Sentiment { get; set; }
	public List<string> KeyPhrases { get; set; } = [];
}