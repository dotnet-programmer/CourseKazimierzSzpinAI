using IntelligentApp.Models.ML_NET;
using IntelligentApp.Services.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace IntelligentApp.Components.Pages.ML_NET;

public partial class CustomerClustering(IFileService fileService)
{
	private string _result = "";
	private string _metrics = "";
	private string _distance = "";
	private string _customersInfo = "";

	private int _age = 50;
	private int _numPurchases = 7;
	private int _avgPurchaseValue = 199;

	private void Start()
	{
		_result = _metrics = _distance = _customersInfo = string.Empty;

		var csvPath = fileService.GetFilePath("data", "ml_net", "customer_data.csv");

		MLContext mlContext = new();

		var data = mlContext.Data.LoadFromTextFile<CustomerData>(
			path: csvPath,
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true);

		var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.5);

		// połączenie kolumn, które będą używane w jedną kolumnę Features
		var pipeline = mlContext.Transforms.Concatenate("Features", "Age", "NumPurchases", "AvgPurchaseValue")
			// normalizacja danych na 0 i 1, jest to spłaszczenie danych żeby nie było przeważających wartości
			.Append(mlContext.Transforms.NormalizeMinMax("Features"))
			.Append(mlContext.Clustering.Trainers.KMeans(
				new KMeansTrainer.Options
				{
					FeatureColumnName = "Features",
					// ilość zbiorów danych
					NumberOfClusters = 2
				}));

		// trenowanie modelu na wszystkich danych
		var model = pipeline.Fit(data);

		// przepuszczenie wszystkich danych przez wytrenowany model uzyskując przewidywania
		var predictions = model.Transform(data);

		var preview = mlContext.Data.CreateEnumerable<ClusteredCustomerData>(predictions, reuseRowObject: false).ToList();

		var grouped = preview
			.GroupBy(x => x.PredictedClusterId)
			.Select(x => new
			{
				ClusterId = x.Key,
				Count = x.Count(),
				AvgAge = x.Average(y => y.Age),
				AvgPurchases = x.Average(y => y.NumPurchases),
				AvgValue = x.Average(y => y.AvgPurchaseValue)
			})
			.ToList();

		foreach (var grp in grouped)
		{
			_customersInfo += $"<br />Klaster {grp.ClusterId}: liczba klientów {grp.Count}, " +
				$"Age avg={grp.AvgAge:F2}, Purchases avg={grp.AvgPurchases:F2}, " +
				$"Value avg={grp.AvgValue:F2}";
		}

		var metrics = mlContext.Clustering.Evaluate(predictions, scoreColumnName: "Score", labelColumnName: null);

		_metrics += $"* Metryki klasteryzacji (KMeans) *<br />";
		// średnia odległość próbek w obrębie ich przypisanych klastrów, im mniejsza wartość tym lepiej
		_metrics += $"---AverageDistance: {metrics.AverageDistance}<br />";
		// zbiór miar czy klastry są rozłączne, im mniejsza wartość tym lepiej
		_metrics += $"---DaviesBouldinIndex:  {metrics.DaviesBouldinIndex}";

		CustomerData sample = new()
		{
			Age = _age,
			NumPurchases = _numPurchases,
			AvgPurchaseValue = _avgPurchaseValue
		};

		var predEngine = mlContext.Model.CreatePredictionEngine<CustomerData, CustomerClusterPrediction>(model);

		var result = predEngine.Predict(sample);

		_result = $"Przewidywany klaster: {result.ClusterId}";

		for (int i = 0; i < result.Distances?.Length; i++)
		{
			_distance += $"<br />Klaster {i + 1}: {result.Distances[i]}";
		}
	}
}