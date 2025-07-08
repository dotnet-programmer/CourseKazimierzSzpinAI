using System.Text;
using IntelligentApp.HttpRepository.Interfaces;

namespace IntelligentApp.HttpRepository;

public class AzureSpeechHttpRepository(HttpClient ttsClient, HttpClient sttClient) : IAzureSpeechHttpRepository
{
	public async Task<byte[]?> GetVoiceAsync(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}

		// treść musi być w formacie Speech Synthetize Markup Language
		var ssml = $@"
            <speak version='1.0' xml:lang='pl=PL'>   
                <voice name='pl-PL-MarekNeural'>
                    {text}
                </voice>
            </speak>
            ";

		// ustawienie nagłówka User-Agent w requeście Header
		ttsClient.DefaultRequestHeaders.UserAgent.Clear();
		ttsClient.DefaultRequestHeaders.Add("User-Agent", "BlazorSpeechSample/1.0");
		
		// przygotowanie zawartości żądania w formacie Application SSML + XML
		var stringContent = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");
		// określenie formatu wyjściowego
		stringContent.Headers.Add("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");

		// wywołanie żądania typu POST w API
		var response = await ttsClient.PostAsync("", stringContent);

		// sprawdzenie błędów
		response.EnsureSuccessStatusCode();

		// odebranie danych audio jako bajty
		return await response.Content.ReadAsByteArrayAsync();
	}
}