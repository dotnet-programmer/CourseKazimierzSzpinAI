using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

// zawiera 2 właściwości, które odpowiadają nagłówkowi z pliku .csv - Text,Label
public class InputData
{
	// atrybuty służą do wskazania, które dane są w której kolumnie
	[LoadColumn(0)]
	public string? Text { get; set; }

	[LoadColumn(1)]
	public string? Label { get; set; }
}