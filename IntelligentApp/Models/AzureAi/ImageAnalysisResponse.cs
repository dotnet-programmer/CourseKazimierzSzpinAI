using System.IO.Pipelines;
using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class ImageAnalysisResponse
{
	[JsonPropertyName("modelVersion")]
	public string ModelVersion { get; set; }

	[JsonPropertyName("metadata")]
	public Metadata Metadata { get; set; }

	[JsonPropertyName("captionResult")]
	public CaptionResult CaptionResult { get; set; }

	[JsonPropertyName("tagsResult")]
	public TagsResult TagsResult { get; set; }

	[JsonPropertyName("readResult")]
	public ReadResult ReadResult { get; set; }
}