using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class TicketData
{
	[LoadColumn(0)]
	public string? Title { get; set; }

	[LoadColumn(1)]
	public string? Description { get; set; }

	[LoadColumn(2)]
	public string? Category { get; set; }
}