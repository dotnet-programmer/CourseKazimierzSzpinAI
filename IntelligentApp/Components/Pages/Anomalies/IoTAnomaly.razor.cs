using IntelligentApp.Models.Anomalies;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.Anomalies;

public partial class IoTAnomaly(IWebHostEnvironment webHostEnvironment)
{
	// lista ze wszystkimi danymi z .csv
	private List<IoTSensorReading> _sensors = [];

	private void DetectAnomalies()
	{
		MLContext mlContext = new();

		// odczytanie danych z .csv i mapowanie wyniku na obiekty IoTSensorReadingInput
		var dataView = mlContext.Data.LoadFromTextFile<IoTSensorReadingInput>(
			path: Path.Combine(webHostEnvironment.WebRootPath, "data", "anomalies", "iot_data.csv"),
			hasHeader: true,
			separatorChar: ','
		);

		var pipeline = mlContext.Transforms
			// złączenie wszystkich kolumn z pliku w pojedynczy wektor cech o nazwie Features
			.Concatenate("Features", "Temperature", "Humidity", "Pressure", "Vibration")
			
			// dodanie normalizacji żeby pomóc w poprawie wyników, czyli wszystkie wartości w kolumnie Features będą w zakresie od 0 do 1, żeby jedna kolumna mająca duże wartości nie zdominowała całego wyniku
			// zakomentowane bo nie wykrywało wszystkich anomalii
			//.Append(mlContext.Transforms.NormalizeMinMax("Features"))

			// wywołanie trenera
			.Append(mlContext.AnomalyDetection.Trainers.RandomizedPca(
				// kolumna z cechami (dane wejściowe do analizy)
				featureColumnName: "Features",
				// liczba głównych składowych, im tutaj jest mniej tym bardziej to uogólnia dane
				rank: 2
			));

		// trenowanie modelu do wykrywania anomalii
		var model = pipeline.Fit(dataView);
		
		// transformacja danych przy użyciu wytrenowanego modelu
		var transformed = model.Transform(dataView);

		// konwersja wyniku transformacji do zbioru obiektów IoTPrediction
		var predictions = mlContext.Data.CreateEnumerable<IoTPrediction>(transformed, reuseRowObject: false);

		// wyświetlenie danych na widoku
		// następuje łączenie danych z pliku .csv (IoTSensorReadingInput) z predykcją 
		_sensors = mlContext.Data.CreateEnumerable<IoTSensorReadingInput>(dataView, false)
			.Zip(predictions, (input, pred) =>
			{
				return new IoTSensorReading
				{
					Timestamp = input.Timestamp,
					Temperature = input.Temperature,
					Humidity = input.Humidity,
					Pressure = input.Pressure,
					Vibration = input.Vibration,
					Prediction = pred
				};
			})
			.ToList();
	}
}