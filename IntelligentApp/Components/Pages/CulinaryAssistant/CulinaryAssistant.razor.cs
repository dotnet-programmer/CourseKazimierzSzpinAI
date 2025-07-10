using System.Globalization;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.CulinaryAssistant;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Components.Pages.CulinaryAssistant;

public partial class CulinaryAssistant(IFileService fileService, IAzureVisionHttpRepository azureVision, IOpenAiHttpRepository openAi)
{
	// do wyświetlenia wybranego zdjecia na widoku bez zapisywania go na dysku
	private string? _imageDataUrl;

	// wybrany plik w postaci tablicy bajtów
	private byte[]? _selectedFileContent;

	// oczekiwanie na ładowanie danych
	private bool _isLoading = false;

	// do wyświetlenia na widoku wszystkich rozpoznanych składników znajdujących się na przesłanym zdjęciu
	private List<string> _ingredients = [];

	// możliwość wprowadzenia nowych składników przez użytkownika - pole do pobierania wartości z formularza
	private string? _ingredient;

	// lista dostępnych typów posiłku
	private List<MealType> _availableMealTypes = [];

	// do przechowania ID wybranego posiłku z listy
	private string? _selectedMealTypeId;

	// lista z proponowanymi daniami
	private List<Meal> _suggestedMeals = [];


	protected override void OnInitialized()
	{
		_availableMealTypes = [
			new() { Id = "breakfast", Name = "Śniadanie" },
			new() { Id = "lunch", Name = "Obiad" },
			new() { Id = "dinner", Name = "Kolacja" },
			new() { Id = "dessert", Name = "Deser" },
		];

		_selectedMealTypeId = _availableMealTypes.FirstOrDefault()?.Id;
	}

	// zdarzenie wywołane po wyborze pliku na widoku
	private async Task OnFileSelectedAsync(InputFileChangeEventArgs e)
	{
		try
		{
			_selectedFileContent = await fileService.ReadInputAsBytesAsync(e.File);
			_imageDataUrl = fileService.GetBase64String(e.File.ContentType, _selectedFileContent);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd podczas wczytywania zdjęcia: {ex.Message}");
			throw;
		}
	}

	// metoda do rozpoznawania składników na zdjęciu
	private async Task IdentifyIngredientsAsync()
	{
		// jeśli żaden plik nie został wybrany to wyjdź z metody
		if (_selectedFileContent == null)
		{
			return;
		}

		_isLoading = true;
		_ingredients.Clear();

		try
		{
			// wywołanie AzureAi który rozpozna obiekty na zdjęciu
			var result = await azureVision.GetImageInfoAsync(_selectedFileContent);

			// pomocnicza zmienna do przechowywania nazw obiektów, które zwrócił serwis Azure
			List<string> objects = [];

			// jeżeli został zwrócony jakiś opis zdjęcia to też zostanie dodany do listy
			if (!string.IsNullOrWhiteSpace(result?.CaptionResult?.Text))
			{
				objects.Add(result.CaptionResult.Text);
			}

			// dodanie do listy składników wszystkich tagów wygenerowanych przez Azure
			if (result?.TagsResult != null)
			{
				objects.AddRange(result.TagsResult.Values
					.Select(t => t.Name)
					.Where(name => !string.IsNullOrWhiteSpace(name)));
			}

			// na podstawie opisu i tagów będzie pobierana informacja o składnikach
			// w tym miejscu wartości są po angielsku, więc trzeba to przetłumaczyć
			// dodatkowo nie wszystkie tagi będą składnikami kulinarnymi, dlatego dodatkowo OpenAi musi sprawdzć czy dany tag jest składnikiem kulinarnym, jeżeli tak to zostanie przetłumaczony i dodany

			// string zawierający opis i wszystkie tagi, który zostanie wysłany w prompcie do OpenAi
			string items = string.Join(",", objects);

			// budowanie prompta - musi mieć dużo szczegółów żeby odpowiedź była precyzyjna
			var prompt = $@"
Przesyłam Ci listę ciągów tekstowych. Twoim zadaniem jest sprawdzić, czy każdy element listy jest konkretnym składnikiem kulinarnym, czyli pojedynczym produktem spożywczym, który można wykorzystać do przygotowania posiłku (np. pomidor, chleb, jabłko, papryka, jajko, ser). Elementy o charakterze ogólnym lub kategorycznym (np. 'grupa żywności', 'warzywo', 'produkty naturalne', 'odżywianie wegańskie') należy pominąć.Jeśli element spełnia warunki, przetłumacz go na język polski. Na końcu zwróć listę tylko tych konkretnych składników, oddzielonych przecinkami, bez dodatkowych słów ani opisu. Moja lista:
{items}
Poproszę tylko o nową listę składników, nic więcej nie dodawaj. Jeżeli Twoim zdaniem nie ma żadnego składnika, który spełnia wszystkie wymagania, to zwróć tylko jeden znak '-', w przeciwnym przypadku lista składników oddzielona przecinkami. Bez żadnego dodatkowo komentarza.
Przykładowy format wyjścia: jabłko,malina,papryka,szynka.
Odpowiedz w języku polskim.";

			// wysłanie prompta do OpenAi i odebranie odpowiedzi w openAiAnswer, odpowiedź w formacie 1 stringa z listą składników oddzielonych przecinkami
			var openAiAnswer = await openAi.AskOpenAiAsync(prompt);

			// rozdzielenie pojedynczego stringa na listę składników
			_ingredients = openAiAnswer
				.Split(",", StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim('-').Trim('.').Trim())
				.ToList();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd analizy obrazu: {ex.Message}");
		}
		finally
		{
			_isLoading = false;
		}
	}

	// metoda do usuwania wybranego składnika z listy wszystkich składników
	private void RemoveIngredient(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			_ingredients.Remove(name);
		}

		//if (string.IsNullOrWhiteSpace(name))
		//{
		//	return;
		//}
		//var ingredientToDelete = _ingredients.FirstOrDefault(x => x == name);
		//if (string.IsNullOrWhiteSpace(ingredientToDelete))
		//{
		//	return;
		//}
		//_ingredients.Remove(ingredientToDelete);
	}

	// metoda do dodania nowego składnika do listy przez użytkownika
	private void AddIngredient()
	{
		if (!string.IsNullOrWhiteSpace(_ingredient))
		{
			_ingredients.Add(_ingredient);
			_ingredient = null;
		}
	}

	// metoda do proponowania dań na podstawie listy składników i wybranego typu posiłku
	private async Task SuggestMealsAsync()
	{
		// jeśli na liście na widoku nie został wybrany żaden typ posiłku albo lista składników jest nullem albo pusta
		if (string.IsNullOrWhiteSpace(_selectedMealTypeId) || _ingredients == null || _ingredients.Count == 0)
		{
			return;
		}

		_isLoading = true;
		_suggestedMeals.Clear();

		try
		{
			// zamiana listy składników na stringa ze skłądnikami rozdzielonymi przecinkiem
			string ingredients = string.Join(",", _ingredients);

			// dodanie w prompcie podwójnych gwiazdek **jakiś tekst** powoduje pogrubienie, czyli wyróżnienie
			var prompt = $@"
Przesyłam Ci listę składników kulinarnych: '{ingredients}'. 
Twoim zadaniem jest wygenerowanie maksymalnie 10 różnych dań/posiłków, które można przygotować, używając wyłącznie tych składników. Ma to być posiłek typu **{_availableMealTypes.FirstOrDefault(x => x.Id == _selectedMealTypeId)?.Name}**. Dla każdego dania podaj dokładnie następujące informacje, oddzielone średnikiem (;) w tej kolejności:
1. Nazwa dania.
2. Czas przygotowania (w minutach).
3. Liczba kalorii w daniu.
4. Ilość białka w daniu (w gramach).
5. Ilość węglowodanów w daniu (w gramach).
6. Ilość tłuszczów w daniu (w gramach).

Każde danie **musi** być oznaczone na początku znakiem gwiazdki (*).
Przykładowy format wyjścia (dla jednego dania):
*Sałatka z pomidorów;15;250;10;30;12

Zwróć **tylko** dane w powyższym formacie, bez dodatkowych komentarzy ani opisu. Jeśli nie da się przygotować dania wyłącznie z podanych składników, pomiń je. Proszę o wygenerowanie wyniku jako pojedynczy string, gdzie poszczególne dania są oddzielone znakiem nowej linii.
Odpowiedz w języku polskim.";

			var openAiAnswer = await openAi.AskOpenAiAsync(prompt);

			// parsowanie stringa zwróconego przez OpenAi na listę proponowanych dań
			var suggestedDishes = ParseDishes(openAiAnswer);

			// dodanie sugerowanych dań do listy wyświetlanej na widoku
			_suggestedMeals.AddRange(suggestedDishes);

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd przy tworzeniu proponowanych dań: {ex.Message}");
		}
		finally
		{
			_isLoading = false;
		}
	}

	private List<Meal> ParseDishes(string input)
	{
		// pobranie ustawień kulturowych do poprawnego parsowania liczb zmiennoprzecinkowych
		var culture = CultureInfo.InvariantCulture;

		// pomocnicza zmienna, która będzie zwracana jako wynik parsowania inputa
		List<Meal> meals = [];

		// podział inputa na linie
		string[] lines = input.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
		
		// przejście po wszystkich liniach z inputa
		foreach (var line in lines)
		{
			// jeżeli dana linia nie zaczyna sie od gwiazdki to nie jest to posiłek, więc przejdź do kolejnej linii
			if (!line.StartsWith('*'))
			{
				continue;
			}

			// usuń znak gwiazdki i otaczajace białe znaki
			string trimmedLine = line.TrimStart('*').Trim();

			// podział pojedynczej linii na części, miejsce podziału wyznacza średnik
			string[] parts = trimmedLine.Split(";", StringSplitOptions.None);

			// jeżeli nie ma 6 elementów to znaczy że czegos brakuje i przejdź do kolejnego
			if (parts.Length != 6)
			{
				continue;
			}

			try
			{
				// utworzenie posiłku ze wszystkimi danymi i dodanie go do listy z posiłkami
				meals.Add(new Meal
				{
					Name = parts[0].Trim(),
					PreparationTime = float.Parse(parts[1].Trim(), culture),
					Calories = float.Parse(parts[2].Trim(), culture),
					Protein = float.Parse(parts[3].Trim(), culture),
					Carbs = float.Parse(parts[4].Trim(), culture),
					Fats = float.Parse(parts[5].Trim(), culture)
				});
			}
			catch (FormatException)
			{
				continue;
			}
		}

		return meals;
	}
}