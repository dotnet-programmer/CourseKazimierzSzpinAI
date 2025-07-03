using System.Text.Json;
using IntelligentApp.Models.AzureAi;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class TextAnalysis
{
	private bool _isLoading = false;
	private bool _showResult = false;
	private string _userText = string.Empty;
	private string _sentimentResult = string.Empty;
	private List<string> _keyPhrases = [];

	[Inject]
	public IHttpClientFactory HttpFactory { get; set; }

	private async Task AnalyzeTextAsync()
	{
		if (string.IsNullOrWhiteSpace(_userText))
		{
			return;
		}

		_isLoading = true;
		_showResult = false;
		_sentimentResult = string.Empty;
		_keyPhrases.Clear();

		var sentiment = await AnalyzeSentimentAsync(_userText);
		var keyPhrases = await ExtractKeyPhrasesAsync(_userText);

		_sentimentResult = sentiment;
		_keyPhrases = keyPhrases;
		_isLoading = false;
		_showResult = true;
	}

	private async Task<string> AnalyzeSentimentAsync(string text)
	{
		try
		{
			var requestBody = new AnalyzeTextRequest
			{
				// rodzaj operacji, czyli ma być przeprowadzona analiza sentymentu
				Kind = "SentimentAnalysis",

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

			var client = HttpFactory.CreateClient("AzureAI");

			// doklejenie adresu endpointa do adresu bazowego klienta HTTP zdefiniowanego w Program.cs
			var endpoint = "language/:analyze-text?api-version=2024-11-01";
			var response = await client.PostAsJsonAsync(endpoint, requestBody);

			// upewnienie się że wszystko się udało, jeśli nie to zostanie zgłoszony wyjątek
			response.EnsureSuccessStatusCode();

			// odczytanie odpowiedzi JSON jako string
			var responseString = await response.Content.ReadAsStringAsync();

			// zdeserializowanie odpowiedzi JSON do obiektu AnalyzeTextResponse
			var analyzeResponse = JsonSerializer.Deserialize<AnalyzeTextResponse>(responseString);

			// odczytanie pierwszego dokumentu z odpowiedzi
			var doc = analyzeResponse?.Results?.Documents?[0];

			// zwrócenie wyniku analizy sentymentu, jeśli nie ma to zwrócenie "unknown"
			return doc?.Sentiment ?? "unknown";
		}
		catch (Exception ex)
		{
			return $"Błąd: {ex.Message}";
		}
	}

	private async Task<List<string>> ExtractKeyPhrasesAsync(string text)
	{
		try
		{
			var requestBody = new AnalyzeTextRequest
			{
				Kind = "KeyPhraseExtraction",
				AnalysisInput = new AnalysisInput
				{
					Documents = new List<AnalysisDocument>
					{
						new AnalysisDocument
						{
							Id = "1",
							Language = "pl",
							Text = text
						}
					}
				},
				Parameters = new Dictionary<string, object>
				{
					{ "modelVersion", "latest" },
					{ "loggingOptOut", false }
				}
			};

			var client = HttpFactory.CreateClient("AzureAI");

			var endpoint = "language/:analyze-text?api-version=2024-11-01";

			var response = await client.PostAsJsonAsync(endpoint, requestBody);
			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync();

			var analyzeResponse = JsonSerializer.Deserialize<AnalyzeTextResponse>(responseString);

			var doc = analyzeResponse?.Results?.Documents?[0];

			return doc?.KeyPhrases ?? new List<string>();
		}
		catch (Exception ex)
		{
			return new List<string> { $"Błąd: {ex.Message}" };
		}
	}
}