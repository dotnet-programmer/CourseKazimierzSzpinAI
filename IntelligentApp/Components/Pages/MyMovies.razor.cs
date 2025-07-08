using IntelligentApp.Models;
using IntelligentApp.Services.Interfaces;

namespace IntelligentApp.Components.Pages;

// można używać uproszczonych konstruktorów zamiast właściwości [Inject]
public partial class MyMovies(IFileReader fileReader)
{
	private readonly List<Movie> _movies = [];
	private readonly string _csvFile = "favourite_movies.csv";

	protected override async Task OnInitializedAsync()
	{
		var lines = await fileReader.ReadAllLinesAsync(_csvFile);
		foreach (var item in lines)
		{
			var values = item.Split(',');
			_movies.Add(new()
			{
				Title = values?[0].Trim('"') ?? string.Empty,
				Year = int.TryParse(values?[1].Trim(), out var year) ? year : 0,
				Genre = values?[2].Trim() ?? string.Empty,
			});
		}
	}
}