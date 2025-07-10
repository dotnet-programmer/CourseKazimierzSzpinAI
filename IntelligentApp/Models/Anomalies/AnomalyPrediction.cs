using Microsoft.ML.Data;

namespace IntelligentApp.Models.Anomalies;

// zrobione dziedziczenie po TimestampData, bo wyniki też zawierają te kolumny, więc dziedziczenie żeby nie powtarzać kodu
public class AnomalyPrediction : AnomalyTimestampData
{
	// informacje o predykcji, zawsze będą zwrócone 3 elementy z wyników detekcji anomalii
	[VectorType(3)]
	public double[]? Prediction { get; set; }
	//Prediction[0] – wartości 0/1 - 1 oznacza, że punkt jest anomalią, 0 oznacza normalność
	//Prediction[1] – "siła" anomalii (im wyższa, tym bardziej nietypowy punkt)
	//Prediction[2] – niewykorzystana wartość, ale zwracana przez ML.NET
}