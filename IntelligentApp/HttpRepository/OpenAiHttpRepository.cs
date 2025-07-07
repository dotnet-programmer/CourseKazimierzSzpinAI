using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.OpenAi;

namespace IntelligentApp.HttpRepository;

// IHttpClientFactory zamienione na HttpClient, wymagana inna konfiguracja w Program.cs
//public class OpenAiHttpRepository(IHttpClientFactory httpClientFactory) : IOpenAiHttpRepository
public class OpenAiHttpRepository(HttpClient httpClient) : IOpenAiHttpRepository
{
	public async Task<string> AskOpenAiAsync(string prompt, string aiModel = "gpt-4", int maxTokens = 100, string endpoint = "")
	{
		try
		{
			var requestBody = new
			{
				// jaki model AI ma być użyty
				model = aiModel,

				// wiadomości do AI
				messages = new[]
				{
					// można wskazać różną rolę wiadomości, np. system, user, assistant
					// rola system definiuje kontekst wiadomości, to definiuje persone tego asystenta
					new { role = "system", content = "Jesteś pomocnym asystentem." },

					// faktyczna wiadomość do przekazania od użytkownika
					new { role = "user", content = prompt }
				},

				// liczba tokenów jaką może wygenerować model w odpowiedzi, wpływa to na długość odpowiedzi i cenę usługi
				max_tokens = maxTokens
			};

			// jako parametr podanie nazwy klienta HTTP zdefiniowanego w Program.cs w AddHttpClient()
			//var httpClient = httpClientFactory.CreateClient("OpenAI");

			// wysłanie żądania do OpenAI
			// pierwszy parametr (endpoint) jest pusty, ponieważ w Program.cs zdefiniowany jest bazowy adres URL klienta HTTP
			using var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);

			// sprawdzenie czy odpowiedź z serwera jest poprawna
			response.EnsureSuccessStatusCode();

			// odczytanie odpowiedzi JSON z serwera i rzutowanie na typ ChatCompletionResponse (z dokumentacji)
			var jsonResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();

			// wyłuskanie odpowiedzi 
			return jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
		}
		catch (Exception ex)
		{
			return $"Błąd: {ex.Message}";
		}
	}
}