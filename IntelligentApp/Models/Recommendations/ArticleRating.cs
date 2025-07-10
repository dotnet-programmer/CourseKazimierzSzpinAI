using Microsoft.ML.Data;

namespace IntelligentApp.Models.Recommendations;

public class ArticleRating
{
	[LoadColumn(0)]
	public int UserId { get; set; }

	[LoadColumn(1)]
	public int ArticleId { get; set; }

	[LoadColumn(2), ColumnName("Label")]
	public float Rating { get; set; }

	[LoadColumn(3)]
	public string? Category { get; set; }
}