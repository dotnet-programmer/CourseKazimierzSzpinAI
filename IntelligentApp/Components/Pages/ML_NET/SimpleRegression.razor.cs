using IntelligentApp.Models.ML_NET;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.ML_NET;

public partial class SimpleRegression(IWebHostEnvironment webHostEnvironment)
{
	private int _titleLength = 600;
	private int _keywordsCount = 4;
	private string _category = "Technology";
	private string _authorExperience = "Newbie";
	private string? _result;
	private string _metrics = string.Empty;

	// w tej metodzie przy każdym uruchomieniu zostaje uruchomione trenowanie modelu
	// w prawdziwych aplikacjach model trenuje się osobno offline albo w dedykowanym procesie np. raz na dzień/tydzień/miesiąc
	// następnie zapisuje się wytrenowany model do pliku i wczytuje się model przy starcie aplikacji aby szybko wykonywać predykcje w pamięci
	private void Start()
	{
		_result = _metrics = string.Empty;

		var webRootPath = webHostEnvironment.WebRootPath;
		var csvPath = Path.Combine(webRootPath, "data", "article_views.csv");

		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<ArticleData>(
			path: csvPath,
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true);

		// 80% danych treningowych, 20% danych testowych
		var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

		// OneHotEncoding - przekształca wartości kategoryczne (czyli takie które można zapisać jak enum) na wartości liczbowe
		// robi się to po to bo większość algorytmów uczenia maszynowego wymaga danych liczbowych a nie tekstowych
		var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CategoryEncoded", inputColumnName: "Category")
			.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AuthorExperienceEncoded", inputColumnName: "AuthorExperience"))
			// połączenie różnych kolumn w 1 kolumnę - wektor o nazwie Features
			.Append(mlContext.Transforms.Concatenate("Features", "TitleLength", "KeywordsCount", "CategoryEncoded", "AuthorExperienceEncoded"))
			// wywołanie trenera regresji
			.Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Views", featureColumnName: "Features"));
		
		// trenowanie modelu danymi treningowymi
		var model = pipeline.Fit(split.TrainSet);

		// sprawdzenie jak model radzi sobie z danymi testowymi
		var predictions = model.Transform(split.TestSet);
		// obliczenie metryk żeby wiedzieć jak poprawny jest model
		var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Views");

		_metrics += $"* Metryki dla regresji (SDCA) *<br />";
		// im mniejsze RMSE tym lepsze dopasowanie, jeżeli wartość 100, to znaczy że predykcja może sie mylić o ok. 100 wyświetleń
		_metrics += $"---RMSE: {metrics.RootMeanSquaredError}<br />";
		// MAE pokazuje średnią różnicę bezwzględną między przewidywaną a rzeczywistą liczbą wyświetleń
		_metrics += $"---MAE:  {metrics.MeanAbsoluteError}<br />";
		// Cooficient of determination, wartość między ujemną a 1, im bliżej 1 tym lepszy model tłumaczący zmienność w danych
		_metrics += $"---R^2:  {metrics.RSquared}";

		ArticleData sample = new()
		{
			TitleLength = _titleLength,
			KeywordsCount = _keywordsCount,
			Category = _category,
			AuthorExperience = _authorExperience
		};

		// input - ArticleData
		// output - ArticlePrediction
		var predEngine = mlContext.Model.CreatePredictionEngine<ArticleData, ArticlePrediction>(model);

		// uruchomienie silnika na przykładzie sample
		var result = predEngine.Predict(sample);

		_result = $"Przewidywana liczba wyświetleń: {result.PredictedViews}";
	}

	// sprawdzenie czy plik modelu istnieje, jeśli nie, to trzeba wytrenować nowy model i zapisać go
	private void StartWithModelFromFile()
	{
		_result = _metrics = string.Empty;

		var webRootPath = webHostEnvironment.WebRootPath;
		var csvPath = Path.Combine(webRootPath, "data", "article_views.csv");
		// ścieżka do pliku z wytrenowanym modelem
		var modelPath = Path.Combine(webRootPath, "data", "article_views_model.zip");

		MLContext mlContext = new();

		ITransformer model;

		//A) Model nie istnieje - trenujemy i zapisujemy
		if (!File.Exists(modelPath))
		{
			var dataView = mlContext.Data.LoadFromTextFile<ArticleData>(
				path: csvPath,
				hasHeader: true,
				separatorChar: ',',
				allowQuoting: true);

			var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

			var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CategoryEncoded", inputColumnName: "Category")
				.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AuthorExperienceEncoded", inputColumnName: "AuthorExperience"))
				.Append(mlContext.Transforms.Concatenate("Features", "TitleLength", "KeywordsCount", "CategoryEncoded", "AuthorExperienceEncoded"))
				.Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Views", featureColumnName: "Features"));

			model = pipeline.Fit(split.TrainSet);

			var predictions = model.Transform(split.TestSet);
			var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Views");

			_metrics += $"* Metryki dla regresji (SDCA) *<br />";
			_metrics += $"---RMSE: {metrics.RootMeanSquaredError}<br />";
			_metrics += $"---MAE:  {metrics.MeanAbsoluteError}<br />";
			_metrics += $"---R^2:  {metrics.RSquared}";

			// zapisanie wytrenowanego modelu do pliku
			mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);
		}
		//B) Model istnieje - trzeba go załadować
		else
		{
			using var stream = File.OpenRead(modelPath);
			model = mlContext.Model.Load(stream, out var inputSchema);
		}

		ArticleData sample = new()
		{
			TitleLength = _titleLength,
			KeywordsCount = _keywordsCount,
			Category = _category,
			AuthorExperience = _authorExperience
		};

		var predEngine = mlContext.Model.CreatePredictionEngine<ArticleData, ArticlePrediction>(model);
		var result = predEngine.Predict(sample);

		_result = $"Przewidywana liczba wyświetleń: {result.PredictedViews}";
	}
}