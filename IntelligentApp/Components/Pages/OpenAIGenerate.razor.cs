using IntelligentApp.Models;
using Microsoft.AspNetCore.Components;

namespace IntelligentApp.Components.Pages;

public partial class OpenAIGenerate
{
	private readonly List<string> _availablePrompts = [];

	private bool _isLoading = false;
	private string _userPrompt = string.Empty;
	private string _generatedText = string.Empty;

	[Inject]
	public IHttpClientFactory HttpFactory { get; set; }

	[Inject]
	public IWebHostEnvironment Env { get; set; }

	protected override async Task OnInitializedAsync()
	{
		var webRootPath = Env.WebRootPath;
		var csvPath = Path.Combine(webRootPath, "data", "prompts.csv");
		var lines = await File.ReadAllLinesAsync(csvPath);

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i].Trim().Trim('"');

			if (!string.IsNullOrWhiteSpace(line))
			{
				_availablePrompts.Add(line);
			}
		}
	}

	private void OnPromptSelected(ChangeEventArgs e)
	{
		var selected = e.Value?.ToString();

		if (!string.IsNullOrWhiteSpace(selected))
		{
			_userPrompt = selected;
		}
	}

	private async Task GenerateTextAsync()
	{
		if (string.IsNullOrWhiteSpace(_userPrompt))
		{
			return;
		}

		_isLoading = true;
		_generatedText = string.Empty;
		_generatedText = await AskOpenAi(_userPrompt);
		_isLoading = false;
	}

	private async Task<string> AskOpenAi(string prompt)
	{
		try
		{
			// jako parametr podanie nazwy klienta HTTP zdefiniowanego w Program.cs w AddHttpClient()
			var client = HttpFactory.CreateClient("OpenAI");

			var requestBody = new
			{
				// jaki model AI ma być użyty
				model = "gpt-4",

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
				max_tokens = 100
			};

			// wysłanie żądania do OpenAI
			// pierwszy parametr (endpoint) jest pusty, ponieważ w Program.cs zdefiniowany jest bazowy adres URL klienta HTTP
			using var response = await client.PostAsJsonAsync("", requestBody);

			if (!response.IsSuccessStatusCode)
			{
				return "Przepraszam, nie udało się uzyskać odpowiedzi od AI";
			}

			// odczytanie odpowiedzi JSON z serwera i rzutowanie na typ ChatCompletionResponse (z dokumentacji)
			var jsonResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();

			// wyłuskanie odpowiedzi 
			var answer = jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

			return answer;
		}
		catch (Exception ex)
		{
			return $"Błąd: {ex.Message}";
		}
	}
}