using Microsoft.ML.Data;

namespace IntelligentApp.Models.ContentAssistant;

// input z csv
public class ProductData
{
	[LoadColumn(0)]
	public float ProductId { get; set; }

	[LoadColumn(1)]
	public string? Name { get; set; }

	[LoadColumn(2)]
	public string? Description { get; set; }
}