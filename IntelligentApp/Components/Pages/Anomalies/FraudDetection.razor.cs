using IntelligentApp.Models.Anomalies;
using IntelligentApp.Services.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace IntelligentApp.Components.Pages.Anomalies;

public partial class FraudDetection(IFileService fileService)
{
	// próg, powyżej którego transakcja będzie uznawana za anomalię
	private const float Threshold = 9000000.0f;
	private List<TransactionsAnomalyResult> _results = [];

	private void DetectFraud()
	{
		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<TransactionsInput>(
			path: fileService.GetFilePath("data", "anomalies", "fraud_transactions.csv"),
			hasHeader: true,
			separatorChar: ',');

		var pipeline = mlContext.Transforms.Concatenate("Features", "Amount")
			// trener klasteryzacji KMeans - polega na znalezieniu centroidu, czyli środków klastrów i przypisaniu do nich punktów w taki sposób, aby minimalizować sumę kwadratów odległości do najbliższego centroidu
			.Append(mlContext.Clustering.Trainers.KMeans(
				new KMeansTrainer.Options
				{
					FeatureColumnName = "Features",
					// wszystkie transakcje będą traktowane jako 1 grupa, i będzie sprawdzane jak daleko każda transakcja odbiega od tego centroidu
					NumberOfClusters = 1,
					MaximumNumberOfIterations = 100
				}));

		// trenowanie modelu KMeans na wszystkich danych 
		var model = pipeline.Fit(dataView);

		// transformacja danych przy użyciu wytrenowanego modelu
		var transformedData = model.Transform(dataView);

		// konwersja wyniku transformacji do zbioru obiektów TransactionsPrediction
		// czyli tutaj znajduje się output z predykcjami
		var predictions = mlContext.Data.CreateEnumerable<TransactionsPrediction>(transformedData, reuseRowObject: false);

		// pobranie oryginalnych danych do transakcji przed przetwarzaniem
		var originalData = mlContext.Data.CreateEnumerable<TransactionsInput>(dataView, reuseRowObject: false);

		// przypisanie wyników do listy, dodatkowo obliczana odległość od centroidu, znajdująca się we właściwości Distances[0]
		// łączone dane z inputu z outputem w celu wyświetlenia wszystkich danych z inputu z ich predykcją
		_results = originalData.Zip(predictions, (input, pred) =>
		{
			float distance = pred.Distances?[0] ?? 0;
			bool isAnomaly = distance > Threshold;

			return new TransactionsAnomalyResult
			{
				TransactionId = input.TransactionId,
				Amount = input.Amount,
				Distance = distance,
				IsAnomaly = isAnomaly
			};
		}).ToList();
	}
}