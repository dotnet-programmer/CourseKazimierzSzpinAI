using IntelligentApp.Models.ML_NET;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.ML_NET;

public partial class SimpleTextClassification(IWebHostEnvironment webHostEnvironment)
{
	private string _opinion = "Uwielbiam ten kurs!";
	private string? _result;

	private void Start()
	{
		if (string.IsNullOrWhiteSpace(_opinion))
		{
			return;
		}

		_result = null;

		// wskazanie ścieżki do pliku z danymi używanymi do trenowania modelu
		var csvPath = Path.Combine(webHostEnvironment.WebRootPath, "data", "simple_text_classification.csv");

		// główny punkt wejscia do ML.NET
		// jako parametr można przekazać seed - stałe ziarno losowości, potrzebne do większej powtarzalności
		MLContext mlContext = new();

		// wczytanie danych z pliku .csv do formatu rozpoznawalnego przez ML.NET
		// InputData - model do zmiany danych z csv na klasę w C#
		var data = mlContext.Data.LoadFromTextFile<InputData>(
			// ścieżka do pliku
			path: csvPath,
			// poinformowanie, czy plik posiada nagłówek 
			hasHeader: true,
			// wskazanie separatora danych
			separatorChar: ',',
			// pozwolenie na wartości znajdujace się w cudzysłowiach
			allowQuoting: true
		);

		// podział danych na treningowe i testowe
		// data - dane używane w modelu
		// testFraction - współczynnik podziału
		// zbiór treningowy służy do wytrenowania modelu, a zbiór testowy do końcowej ewaluacji jakości modelu
		var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.5);

		// tworzenie pipeline, czyli potok przetwarzania danych,
		// czyli taki łańcuch operacji od przetwarzania tekstu przez konwersję etykiety po trenowanie klasyfikatora i mapowanie wyniku
		// 1. zamiana kolumny "Text" typu string na wektor liczb, czyli Feature, który później jest przekazywany do algorytmu
		var pipeline = mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Text", outputColumnName: "Features")
			// 2. żeby zrobić więcej rzeczy, trzeba dokleić metodę .Append()
			// tutaj dane z kolumny Label mająca 2 wartości (pozytywna/negatywna) konwertowane są na wartości klucza 1/0
			.Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Label", outputColumnName: "LabelKey"))
			// 3. wywołanie algorytmu klasyfikacji wieloklasowej
			.Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("LabelKey", "Features"))
			// 4. odwrotne mapowanie z klucza na etykietę, czyli tutaj będzie wynik tej predykcji
			.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

		// trenowanie modelu - w tym miejscu ML.NET dopasowuje parametry do danych, by nauczyć się rozróżniać teksty pozytywne od negatywnych
		// jako parametr przekazanie zbioru danych treningowych 
		var model = pipeline.Fit(split.TrainSet);

		// aby ocenić model, trzeba przepuścić zbiór testowy przez wytrenowany model i zmierzyć jego skuteczność
		// w odpowiedzi otrzymuje się przewidywaną predykcję
		var predictions = model.Transform(split.TestSet);

		// przekazanie predykcji aby obliczyć różną miarę jakości dla klasyfikacji wieloklasowej
		// sprawdzenie jakości wytrenowanego modelu
		var metrics = mlContext.MulticlassClassification.Evaluate(
			predictions,
			// wskazanie nazwy kolumny Label
			labelColumnName: "LabelKey",
			// wskazanie nazwy kolumny predicted
			predictedLabelColumnName: "PredictedLabel"
		);

		// utworzenie silnika predykcji, dzięki niemu można robić łatwe predykcje przekazując np. opinie od użytkownika i prosząc o wynik
		var predEngine = mlContext.Model.CreatePredictionEngine<InputData, Prediction>(model);

		// przewidywanie wyniku dla podanego inputa przez klienta
		// wynikiem jest przewidywanie czy jest to opinia pozytywna czy negatywna
		var result = predEngine.Predict(new InputData { Text = _opinion });

		// przypisanie wyniku do pola na widoku
		_result = $"Opinia: {_opinion} --- {result.PredictedLabel} --- Accuracy: {metrics.MicroAccuracy:P2}";
	}
}