namespace IntelligentApp.Models.Anomalies;

// klasa pomocnicza do wyświetlania wyników na widoku
public class TransactionsAnomalyResult
{
	public float TransactionId { get; set; }
	public float Amount { get; set; }
	// odległość od centroidu
	public float Distance { get; set; }
	public bool IsAnomaly { get; set; }
}