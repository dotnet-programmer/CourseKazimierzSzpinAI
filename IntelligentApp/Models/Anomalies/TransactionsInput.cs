using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

// klasa Input z danymi z pliku .csv
public class TransactionsInput
{
	[LoadColumn(0)]
	public float TransactionId { get; set; }
	
	[LoadColumn(1)]
	public float Amount { get; set; }
}