using IntelligentApp.Models.Recommendations;
using IntelligentApp.Services.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace IntelligentApp.Components.Pages.Recommendations;

// Rekomendacja produktów (collaborative)
public partial class Recommendations(IFileService fileService)
{
	private readonly string _csvPath = fileService.GetFilePath("data", "recommendations", "product_ratings.csv");
	private readonly string _modelPath = fileService.GetFilePath("data", "recommendations", "product_ratings_model.zip");
	
	private List<ProductRecommendationResult>? _recommendedProducts;
	private int _selectedUserId = 1;
	private string? _trainModelInfo;

	private void TrainModel()
	{
		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<ProductRating>(
			path: _csvPath,
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true);

		var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

		// parametry przekazywane do trenera w mlContext.Recommendation().Trainers.MatrixFactorization(options)
		var options = new MatrixFactorizationTrainer.Options
		{
			// nazwa kolumny w danych która reprezentuje index kolumny w macierzy,
			// czyli oznaczenie że informacje z kolumny UserId będą traktowane jako indeks kolumny w macierzy,
			// czyli wskazanie dla którego użytkownika tworzona jest rekomendacja
			MatrixColumnIndexColumnName = "UserId",

			// nazwa kolumny która reprezentuje index wiersza w macierzy, czyli drugi wymiar w macierzy, czyli który produkt będzie wskazany
			MatrixRowIndexColumnName = "ProductId",

			// wskazanie nazwy kolumny w zbiorze danych, która zawiera etykietę, czyli tą wartość do przewidzenia 
			// w klasie ProductRating jest właściwość Rating z atrybutem ColumnName("Label") i to ona jest tutaj wskazana
			LabelColumnName = "Label",

			// liczba iteracji algorytmu uczącego, im więcej tym dokładniej model może się dopasować, ale wydłuża to czas treningu
			NumberOfIterations = 20,

			// liczba wymiarów w wektorach ukrytych, które opisują użytkowników i produkty,
			// czyli tutaj określa się jak głęboko model będzie w stanie uchwycić złożoność danych
			// większa wartość pomoże w lepszym uchwyceniu relacji danych, ale rodzi to ryzyko przeuczenia i przedłuża czas treningu
			// 100 oznacza, że każdy użytkownik i każdy produkt będa reprezentowane za pomocą 100 wymiarowych wektorów ukrytych
			ApproximationRank = 100
		};

		// żeby użyć MatrixFactorization trzeba kolumny z danymi zamienić na klucze
		var pipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "UserId", outputColumnName: "UserId")
			.Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "ProductId", outputColumnName: "ProductId"))
			.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

		// trenowanie modelu na danych treningowych
		var model = pipeline.Fit(split.TrainSet);

		// weryfikacja jakości modelu
		var predictions = model.Transform(split.TestSet);
		var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

		// zapisanie wytrenowanego modelu do pliku
		mlContext.Model.Save(model, split.TrainSet.Schema, _modelPath);

		_trainModelInfo = "Model został wytrenowany i zapisany do pliku.";
	}

	// 1. wywołanie predykcji dla użytkownika na każdym produkcie
	// 2. wyświetlenie rekomendacji
	private void GenerateRecommendations()
	{
		// w realnej aplikacji może być pobranie ID wszystkich produktów z bazy danych
		List<int> productIds = [101, 102, 103, 104, 105];
		List<ProductRecommendationResult> predictions = [];

		foreach (var productId in productIds)
		{
			var score = Predict(_selectedUserId, productId);

			predictions.Add(new ProductRecommendationResult
			{
				ProductId = productId,
				PredictedScore = score
			});
		}

		_recommendedProducts = predictions.OrderByDescending(x => x.PredictedScore).ToList();
	}

	private float Predict(int userId, int productId)
	{
		if (!File.Exists(_modelPath))
		{
			return 0;
		}

		using var stream = File.OpenRead(_modelPath);

		MLContext mlContext = new();

		var model = mlContext.Model.Load(stream, out var inputSchema);

		var predEngine = mlContext.Model.CreatePredictionEngine<ProductRating, ProductPrediction>(model);

		ProductRating productRating = new() { UserId = userId, ProductId = productId };

		return predEngine.Predict(productRating).Score;
	}
}