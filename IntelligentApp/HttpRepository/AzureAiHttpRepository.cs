using System.Text.Json;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.AzureAi;

namespace IntelligentApp.HttpRepository;

public class AzureAiHttpRepository(IHttpClientFactory httpClientFactory) : IAzureAiHttpRepository
{
	public async Task<string> AnalyzeSentimentAsync(string text)
	{
		try
		{
			var response = await AnalyzeText(text, AzureRequestKind.SentimentAnalysis);

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
			var response = await AnalyzeText(text, AzureRequestKind.KeyPhraseExtraction);

			// odczytanie pierwszego dokumentu z odpowiedzi i zwrócenie słów kluczowych, jeśli nie ma to zwrócenie pustej listy
			return response?[0]?.KeyPhrases ?? [];
		}
		catch (Exception ex)
		{
			return [$"Błąd: {ex.Message}"];
		}
	}

	private async Task<List<AnalyzeTextDocument>?> AnalyzeText(string text, AzureRequestKind requestType)
	{
		var requestBody = new AnalyzeTextRequest
		{
			// rodzaj operacji
			Kind = requestType.ToString(),

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

		var httpClient = httpClientFactory.CreateClient("AzureAI");

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