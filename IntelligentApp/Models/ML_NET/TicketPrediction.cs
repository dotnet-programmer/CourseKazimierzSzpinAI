using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class TicketPrediction
{
	[ColumnName("PredictedLabel")]
	public string? PredictedCategory { get; set; }
}