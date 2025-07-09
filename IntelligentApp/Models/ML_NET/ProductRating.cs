using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ProductRating
{
	[LoadColumn(0)]
	public int UserId { get; set; }

	[LoadColumn(1)]
	public int ProductId { get; set; }

	[LoadColumn(2), ColumnName("Label")]
	public float Rating { get; set; }
}