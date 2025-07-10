using IntelligentApp.Models.Recommendations;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.Recommendations;

// Rekomendacje filmów (content-based)
public partial class MoviesRecommendations(IWebHostEnvironment webHostEnvironment)
{
	private readonly string _csvPath = Path.Combine(webHostEnvironment.WebRootPath, "data", "recommendations", "movies.csv");

	private List<MovieData> _allMovies;
	private List<SimilarMovie> _similarMovies;
	private List<MovieVector> _movieVectors;
	private float? _selectedMovieId;
	private string _selectedMovieTitle = string.Empty;
	private MLContext _mlContext;
	private IDataView _dataView;

	protected override void OnInitialized()
	{
		_mlContext = new();

		// wczytanie wszystkich filmów i wyświetlenie na liście na widoku
		LoadMovies();

		// przekształcenie danych tekstowych: Title, Genre, Description w wektory cech, które będą zrozumiałe dla modeli ML
		TransformMoviesToVectors();
	}

	// wczytanie wszystkich filmów i wyświetlenie na liście na widoku
	private void LoadMovies()
	{
		_dataView = _mlContext.Data.LoadFromTextFile<MovieData>(
			path: _csvPath,
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true);

		// reuseRowObject - false oznacza że będzie nowy obiekt dla każdego rzędu
		_allMovies = _mlContext.Data.CreateEnumerable<MovieData>(_dataView, reuseRowObject: false).ToList();
	}

	// przekształcenie danych tekstowych: Title, Genre, Description w wektory cech, które będą zrozumiałe dla modeli ML
	private void TransformMoviesToVectors()
	{
		// złączenie kolumn Title, Genre, Description w 1 kolumnę wyjściową o nazwie "TextInput"
		var pipeline = _mlContext.Transforms.Concatenate("TextInput", "Title", "Genre", "Description")
			// zamiana inputu na wektor cech
			.Append(_mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: "TextInput"));

		// budowanie modelu transformacji na podstawie _dataView
		var transform = pipeline.Fit(_dataView);
		var transformData = transform.Transform(_dataView);

		// utworzenie listy z wektorami cech
		var features = _mlContext.Data.CreateEnumerable<TransformedMovie>(transformData, reuseRowObject: false).ToList();

		// złączenie wektorów cech z listą wszystkich filmów
		// dla każdego filmu będzie przypisana tablica z wektorem cech
		_movieVectors = features
			.Zip(_allMovies, (f, m) => new MovieVector
			{
				MovieId = m.MovieId,
				Features = f.Features
			})
			.ToList();
	}

	// wyświetlenie rekomendacji - metoda przypisana do przycisku na formularzu
	private void ShowRecommendations()
	{
		if (_selectedMovieId == null)
		{
			return;
		}

		// sprawdzenie jaki film został wybrany przez użytkownika i pobranie jego danych
		var selectedMovie = _allMovies.FirstOrDefault(x => x.MovieId == _selectedMovieId.Value);

		if (selectedMovie == null)
		{
			return;
		}

		// wyświetlenie tytułu filmu na widoku
		_selectedMovieTitle = selectedMovie.Title;

		// pobranie listy podobnych filmów
		_similarMovies = GetSimilarMovies(_selectedMovieId.Value, 3);
	}

	// zwraca podobne filmy na podstawie przekazanego Id filmu, + określenie ile filmów ma zostać zwrócone
	// znajduje film docelowy o ID=movieId, pobiera jego wektor cech z listy _movieVectors
	// obliczenie miary podobieństwa tzw. cosinusowego podobieństwa między dwoma wektorami
	// posortowanie malejąco i zwrócenie topN rekordów
	private List<SimilarMovie> GetSimilarMovies(float movieId, int topN = 3)
	{
		var targetMovie = _movieVectors.FirstOrDefault(x => x.MovieId == movieId);

		if (targetMovie == null)
		{
			return [];
		}

		List<SimilarMovie> similarities = [];

		foreach (var movieVector in _movieVectors)
		{
			if (movieVector.MovieId == movieId)
			{
				continue;
			}

			// obliczenie podobieństwa
			// targetMovie.Features - wektor cech z docelowego filmu
			// movieVector.Features - wektor cech z danej iteracji
			var sim = CosineSimilarity(targetMovie.Features, movieVector.Features);

			// pobranie informacji o filmie z listy
			var movieInfo = _allMovies.First(x => x.MovieId == movieVector.MovieId);
			
			// dodanie filmu z określonym podobieństwem do listy podobnych filmów
			similarities.Add(new SimilarMovie { Movie = movieInfo, Similarity = sim });
		}

		// posortowanie i wybranie topN rekordów
		return similarities.OrderByDescending(x => x.Similarity).Take(topN).ToList();
	}

	private float CosineSimilarity(float[] vecA, float[] vecB)
	{
		var dot = 0f;
		var magA = 0f;
		var magB = 0f;

		for (int i = 0; i < vecA.Length; i++)
		{
			dot += vecA[i] * vecB[i];
			magA += vecA[i] * vecA[i];
			magB += vecB[i] * vecB[i];
		}

		magA = (float)Math.Sqrt(magA);
		magB = (float)Math.Sqrt(magB);

		return magA == 0 || magB == 0 ? 0f : dot / (magA * magB);
	}
}