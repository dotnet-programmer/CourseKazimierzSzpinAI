using IntelligentApp.Models;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class MyMovies
{
	private readonly List<Movie> _movies = [];
	private string? _csvFilePath;

	[Inject]
	public IWebHostEnvironment WebHostEnv { get; set; }

	protected override async Task OnInitializedAsync()
	{
		_csvFilePath = Path.Combine(WebHostEnv.WebRootPath, "data", "favourite_movies.csv");

		var lines = await File.ReadAllLinesAsync(_csvFilePath);
		for (int i = 1; i < lines.Length; i++)
		{
			var values = lines[i].Split(',');
			_movies.Add(new()
			{
				Title = values?[0].Trim('"') ?? string.Empty,
				Year = int.TryParse(values?[1].Trim(), out var year) ? year : 0,
				Genre = values?[2].Trim() ?? string.Empty,
			});
		}
	}
}