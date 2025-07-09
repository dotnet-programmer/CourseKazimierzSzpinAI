using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ClusteredCustomerData
{
	[LoadColumn(0)]
	public float CustomerId { get; set; }

	[LoadColumn(1)]
	public float Age { get; set; }

	[LoadColumn(2)]
	public float NumPurchases { get; set; }

	[LoadColumn(3)]
	public float AvgPurchaseValue { get; set; }

	[ColumnName("PredictedLabel")]
	public uint PredictedClusterId { get; set; }

	[ColumnName("Score")]
	public float[]? Distances { get; set; }
}