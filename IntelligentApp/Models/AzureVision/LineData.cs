namespace IntelligentApp.Models.AzureVision;

// Dzięki tym punktom można wyznaczyć obszar, gdzie znajduje się cały tekst danej linii.
// Będą to współrzędne prostokąta, który całkowicie otacza daną linię tekstu. 
public class LineData
{
	public string Text { get; set; }
	public float MinX { get; set; }
	public float MinY { get; set; }
	public float MaxX { get; set; }
	public float MaxY { get; set; }
}