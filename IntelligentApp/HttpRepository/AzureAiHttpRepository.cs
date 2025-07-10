using System.Text.Json;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.AzureAi;
using IntelligentApp.Models.AzureVision;

namespace IntelligentApp.HttpRepository;

// IHttpClientFactory zamienione na HttpClient, wymagana inna konfiguracja w Program.cs
//public class AzureAiHttpRepository(IHttpClientFactory httpClientFactory) : IAzureAiHttpRepository
public class AzureAiHttpRepository(HttpClient httpClient) : IAzureAiHttpRepository
{
	public async Task<string> AnalyzeSentimentAsync(string text)
	{
		try
		{
			var response = await AnalyzeTextAsync(text, AzureRequestKind.SentimentAnalysis);

			// odczytanie pierwszego dokumentu z odpowiedzi i zwrócenie wyniku analizy sentymentu, jeśli nie ma to zwrócenie "unknown"
			return response?[0]?.Sentiment ?? "unknown";
		}
		catch (Exception ex)
		{
			return $"Błąd: {ex.Message}";
		}
	}

	public async Task<List<string>> ExtractKeyPhrasesAsync(string text)
	{
		try
		{
			var response = await AnalyzeTextAsync(text, AzureRequestKind.KeyPhraseExtraction);

			// odczytanie pierwszego dokumentu z odpowiedzi i zwrócenie słów kluczowych, jeśli nie ma to zwrócenie pustej listy
			return response?[0]?.KeyPhrases ?? [];
		}
		catch (Exception ex)
		{
			return [$"Błąd: {ex.Message}"];
		}
	}

	// na podstawie przesłanej tablicy bajtów zwróci informacje o obrazie
	public async Task<ImageAnalysisResponse?> GetImageInfoAsync(byte[] image)
	{
		// parametr features określa co ma zostać zrobione z obrazem
		// caption - opis obrazu
		// tags - lista tagów
		// read - OCR - odczytanie tekstu z obrazu
		var endpoint = "computervision/imageanalysis:analyze?api-version=2024-02-01&features=caption,tags, read";

		// zawartość żądania to surowe dane binarne
		using var content = new ByteArrayContent(image);
		content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

		var response = await httpClient.PostAsync(endpoint, content);
		response.EnsureSuccessStatusCode();

		var responseString = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<ImageAnalysisResponse>(responseString);
	}

	private async Task<List<AnalyzeTextDocument>?> AnalyzeTextAsync(string text, AzureRequestKind requestKind)
	{
		var requestBody = new AnalyzeTextRequest
		{
			// rodzaj operacji
			Kind = requestKind.ToString(),

			// dane wejściowe z listą dokumentów do analizy
			AnalysisInput = new AnalysisInput
			{
				Documents = new List<AnalysisDocument>
				{
					new AnalysisDocument
					{
						// każde ID musi być unikatowe, tutaj tylko jeden dokument
						Id = "1",

						// określenie języka dokumentu, w tym przypadku polski
						Language = "pl",

						// tekst do analizy sentymentu
						Text = text
					}
				}
			},

			// możliwość przekazania parametrów:
			// - wersji modelu
			// - opcje logowania danych przez Microsoft (wykorzystywane do poprawy jakości modeli AI)
			Parameters = new Dictionary<string, object>
			{
				{ "modelVersion", "latest" },
				{ "loggingOptOut", false }
			}
		};

		//var httpClient = httpClientFactory.CreateClient("AzureAI");

		// doklejenie adresu endpointa do adresu bazowego klienta HTTP zdefiniowanego w Program.cs
		var endpoint = "language/:analyze-text?api-version=2024-11-01";

		// wysłanie żądania POST z ciałem JSON do usługi Azure AI
		var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);

		// upewnienie się że wszystko się udało, jeśli nie to zostanie zgłoszony wyjątek
		response.EnsureSuccessStatusCode();

		// odczytanie odpowiedzi JSON jako string
		var responseString = await response.Content.ReadAsStringAsync();

		// zdeserializowanie odpowiedzi JSON do obiektu AnalyzeTextResponse
		var analyzeResponse = JsonSerializer.Deserialize<AnalyzeTextResponse>(responseString);

		// odczytanie wyników analizy z odpowiedzi i zwrócenie listy dokumentów
		return analyzeResponse?.Results?.Documents;
	}
}