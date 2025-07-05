using IntelligentApp.Models;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class MyMovies
{
	private readonly List<Movie> _movies = [];
	private readonly string _csvFile = "favourite_movies.csv";

	[Inject]
	public IFileReader FileReader { get; set; }

	protected override async Task OnInitializedAsync()
	{
		var lines = await FileReader.ReadAllLinesAsync(_csvFile);
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