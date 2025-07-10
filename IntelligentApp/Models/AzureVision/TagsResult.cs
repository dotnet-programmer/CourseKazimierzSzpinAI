using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureVision;

public class TagsResult
{
	[JsonPropertyName("values")]
	public List<TagValue> Values { get; set; }
}