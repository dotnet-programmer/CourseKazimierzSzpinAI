using IntelligentApp.Models.Anomalies;
using IntelligentApp.Services.Interfaces;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.Anomalies;

public partial class Anomaly(IFileService fileService)
{
	private List<AnomalyPrediction>? _logs;

	private void DetectAnomaly()
	{
		MLContext mlContext = new();

		var dataView = mlContext.Data.LoadFromTextFile<AnomalyTimestampData>(
			path: fileService.GetFilePath("data", "anomalies", "simple_logs.csv"),
			hasHeader: true,
			separatorChar: ',',
			allowQuoting: true
			);

		var pipeline = mlContext.Transforms.DetectAnomalyBySrCnn(
			// nazwa właściwości Prediction z klasy AnomalyPrediction stworzonej do wyników predykcji
			outputColumnName: "Prediction",

			// nazwa właściwości Value z danymi wejściowymi do analizy z klasy TimestampData stworzonej do mapowania danych z csv na klasę C#
			inputColumnName: "Value",

			// próg czułości anomalii, im niższy tym więcej anomalii wykrywa
			threshold: 0.35f,

			// okno do wykrywania anomalii, liczba poprzednich wartości, na których model bazuje, im większa wartość tym bardziej model będzie analizował ogólny wzorzec a nie pojedyncze skoki
			windowSize: 8,

			// uwzględnianie poprzednich wartości do wyliczania trendu, pomaga to modelowi zrozumieć przeszłe wartości i wyznaczyć poprawne trendy
			backAddWindowSize: 3,

			// uwzględnianie wartości z przyszłości
			lookaheadWindowSize: 3,

			// okno do podejmowania decyzji o anomalii, określa na ilu wartościach model powinien bazować podczas podejmowania decyzji o anomalii
			judgementWindowSize: 8
		);

		// trenowanie modelu
		var transform = pipeline.Fit(dataView);

		// transferowanie danych przy użyciu wytrenowanego modelu
		var transformData = transform.Transform(dataView);

		// konwertowanie wyniku transformacji do zbioru obiektów AnomalyPrediction, czyli klasy pomocniczej do wyników predykcji
		var predictions = mlContext.Data.CreateEnumerable<AnomalyPrediction>(transformData, reuseRowObject: false);

		// przypisanie zbioru predukcji do listy wyświetlającej dane na widoku
		_logs = predictions.ToList();
	}
}