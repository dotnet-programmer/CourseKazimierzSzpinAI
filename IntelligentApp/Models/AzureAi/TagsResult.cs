using System.Text.Json.Serialization;

namespace IntelligentApp.Models.AzureAi;

public class TagsResult
{
	[JsonPropertyName("values")]
	public List<TagValue> Values { get; set; }
}