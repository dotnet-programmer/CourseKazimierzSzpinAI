using Microsoft.ML.Data;

namespace IntelligentApp.Models.Recommendations;

// do mapowania obiektów w C# z pliku .csv
public class MovieData
{
	[LoadColumn(0)]
	public float MovieId { get; set; }

	[LoadColumn(1)]
	public string? Title { get; set; }

	[LoadColumn(2)]
	public string? Genre { get; set; }

	[LoadColumn(3)]
	public string? Description { get; set; }
}