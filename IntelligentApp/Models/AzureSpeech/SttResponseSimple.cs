namespace IntelligentApp.Models.AzureSpeech;

// dane zwrócone z zapytania do Azure Speech Service w formacie Simple
public class SttResponseSimple
{
	public string RecognitionStatus { get; set; }
	public string DisplayText { get; set; }
	public long Offset { get; set; }
	public long Duration { get; set; }
}