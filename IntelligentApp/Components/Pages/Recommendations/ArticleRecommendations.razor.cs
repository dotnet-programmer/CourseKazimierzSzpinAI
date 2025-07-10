using IntelligentApp.Models.Recommendations;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace IntelligentApp.Components.Pages.Recommendations;

// Rekomendacja artykułów (collaborative + podział na kategorie (propozycje tylko z 1 wybranej kategorii))
// na podstawie Id użytkownika oraz wybranej kategorii zwróci najbardziej rekomendowane artykuły
// plik article_ratings.csv zawiera Id użytkownika, Id artykułu, ocenę (opisaną jako Label) i jeszcze kategoria, ale bez opisu w 1 wierszu
public partial class ArticleRecommendations(IWebHostEnvironment webHostEnvironment)
{
	private readonly string _csvPath = Path.Combine(webHostEnvironment.WebRootPath, "data", "recommendations", "article_ratings.csv");
	private readonly string _modelPath = Path.Combine(webHostEnvironment.WebRootPath, "data", "recommendations", "article_ratings_model.zip");

	private List<ArticleRecommendationResult> _recommendedArticle = [];
	private List<string?> _availableCategories = [];
	private List<ArticleRating> _allArticles = [];
	private string _trainModelInfo = string.Empty;
	private string _selectedCategory = "Programming";
	private int _selectedUserId = 1;

	protected override void OnInitialized()
	{
		_allArticles = LoadCsvData();

		_availableCategories = _allArticles
			.Select(x => x.Category)
			.Distinct()
			.OrderBy(x => x)
			.ToList();
	}

	private List<ArticleRating> LoadCsvData()
	{
		List<ArticleRating> ratings = [];

		if (!File.Exists(_csvPath))
		{
			return ratings;
		}

		var lines = File.ReadAllLines(_csvPath).Skip(1);

		foreach (var line in lines)
		{
			var parts = line.Split(',');

			var userId = int.Parse(parts[0]);
			var articleId = int.Parse(parts[1]);
			var rating = float.Parse(parts[2]);
			var category = parts[3];

			ratings.Add(new ArticleRating
			{
				UserId = userId,
				ArticleId = articleId,
				Rating = rating,
				Category = category
			});
		}
		return ratings;
	}

	private void TrainModel()
	{
		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<ArticleRating>(
			path: _csvPath,
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true);

		var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

		var options = new MatrixFactorizationTrainer.Options
		{
			MatrixColumnIndexColumnName = "UserId",
			MatrixRowIndexColumnName = "ArticleId",
			LabelColumnName = "Label",
			NumberOfIterations = 20,
			ApproximationRank = 100
		};

		var pipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "UserId", outputColumnName: "UserId")
			.Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "ArticleId", outputColumnName: "ArticleId"))
			.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

		var model = pipeline.Fit(split.TrainSet);

		var predictions = model.Transform(split.TestSet);
		var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

		mlContext.Model.Save(model, split.TrainSet.Schema, _modelPath);

		_trainModelInfo = "Model został wytrenowany i zapisany do pliku.";
	}

	private void GenerateRecommendations()
	{
		var filteredArticles = _allArticles
			.Where(x => x.Category == _selectedCategory)
			.Select(x => x.ArticleId)
			.Distinct()
			.ToList();

		List<ArticleRecommendationResult> predictions = [];

		foreach (var articleId in filteredArticles)
		{
			var score = Predict(_selectedUserId, articleId);

			predictions.Add(new ArticleRecommendationResult
			{
				ArticleId = articleId,
				PredictedScore = score
			});
		}

		_recommendedArticle = predictions.OrderByDescending(p => p.PredictedScore).ToList();
	}

	private float Predict(int userId, int articleId)
	{
		if (!File.Exists(_modelPath))
		{
			return 0;
		}

		using var stream = File.OpenRead(_modelPath);

		MLContext mlContext = new();
		var model = mlContext.Model.Load(stream, out var inputSchema);

		var predEngine = mlContext.Model.CreatePredictionEngine<ArticleRating, ArticlePrediction>(model);

		var articleRating = new ArticleRating
		{
			UserId = userId,
			ArticleId = articleId
		};

		return predEngine.Predict(articleRating).Score;
	}
}