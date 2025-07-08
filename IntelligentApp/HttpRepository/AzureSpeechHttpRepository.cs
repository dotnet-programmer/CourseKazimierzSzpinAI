using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.AzureSpeech;

namespace IntelligentApp.HttpRepository;

public class AzureSpeechHttpRepository(HttpClient ttsClient, HttpClient sttClient) : IAzureSpeechHttpRepository
{
	// zwraca wygenerowane audio w formacie tablicy bajtów na podstawie przesłanego tekstu
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

	// zwraca tekst wygenerowany na podstawie przesłanego audio
	public async Task<string?> GetTextAsync(byte[] audioData)
	{
		if (audioData == null || audioData.Length == 0)
		{
			return null;
		}

		// przygotowanie treści w jakims formacie, tutaj audio/wav ale może być dowolny (mp4, mp3 itp)
		using var content = new ByteArrayContent(audioData);
		content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

		// wywołanie żądania typu POST w API
		// do adresu endpointa trzeba dokleić parametry, tutaj język i format (simple/detailed)
		var response = await sttClient.PostAsync("?language=pl-PL&format=simple", content);

		// sprawdzenie błędów
		response.EnsureSuccessStatusCode();

		// odczytanie odpowiedzi w formacie string
		var reponseString = await response.Content.ReadAsStringAsync();

		// deserializacja JSON na zwracany model - SttResponseSimple
		var sttResponse = JsonSerializer.Deserialize<SttResponseSimple>(reponseString);

		return sttResponse?.RecognitionStatus == "Success" ? sttResponse.DisplayText : null;
	}
}