using Microsoft.ML.Data;

namespace IntelligentApp.Models.ML_NET;

public class ArticleData
{
	[LoadColumn(1)]
	public float TitleLength { get; set; }

	[LoadColumn(2)]
	public float KeywordsCount { get; set; }

	[LoadColumn(3)]
	public string? Category { get; set; }

	[LoadColumn(4)]
	public string? AuthorExperience { get; set; }

	[LoadColumn(5)]
	public float Views { get; set; }
}