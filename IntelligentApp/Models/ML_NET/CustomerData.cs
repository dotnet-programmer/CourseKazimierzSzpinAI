using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class CustomerData
{
	[LoadColumn(0)]
	public float CustomerId { get; set; }

	[LoadColumn(1)]
	public float Age;

	[LoadColumn(2)]
	public float NumPurchases;

	[LoadColumn(3)]
	public float AvgPurchaseValue;
}