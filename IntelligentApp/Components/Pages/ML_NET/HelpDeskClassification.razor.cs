using IntelligentApp.Models.ML_NET;
using IntelligentApp.Services.Interfaces;
using Microsoft.ML;

namespace IntelligentApp.Components.Pages.ML_NET;

public partial class HelpDeskClassification(IFileService fileService)
{
	private string? _title;
	private string? _description;
	private string? _result;
	private string _metrics = string.Empty;

	// na podstawie przysłanego zgłoszenia użytkownika system ma przypisać odpowiednią kategorię
	private void Start()
	{
		if (string.IsNullOrWhiteSpace(_title) || string.IsNullOrWhiteSpace(_description))
		{
			return;
		}

		_result = string.Empty;

		var csvPath = fileService.GetFilePath("data", "ml_net", "helpdesk_tickets.csv");
		var modelPath = fileService.GetFilePath("data", "ml_net", "helpdesk_tickets_model.zip");

		MLContext mlContext = new();
		ITransformer model;

		if (!File.Exists(modelPath))
		{
			var dataView = mlContext.Data.LoadFromTextFile<TicketData>(
				path: csvPath,
				hasHeader: true,
				separatorChar: ',',
				allowQuoting: true);

			var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

			var pipeline = mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Title", outputColumnName: "TitleFeaturized")
					.Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Description", outputColumnName: "DescriptionFeaturized"))
					.Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
					.Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Category", outputColumnName: "LabelKey"))
					.Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("LabelKey", "Features"))
					.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

			model = pipeline.Fit(split.TrainSet);

			var predictions = model.Transform(split.TestSet);

			var metrics = mlContext.MulticlassClassification.Evaluate(
				predictions,
				labelColumnName: "LabelKey",
				predictedLabelColumnName: "PredictedLabel"
			);

			_metrics += $"* Metryki dla klasyfikacji *<br />";
			_metrics += $"---MicroAccuracy: {metrics.MicroAccuracy:P2}<br />";
			_metrics += $"---MacroAccuracy:  {metrics.MacroAccuracy:P2}<br />";
			_metrics += $"---LogLoss:  {metrics.LogLoss:P2}";

			mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);
		}
		else
		{
			using var stream = File.OpenRead(modelPath);
			model = mlContext.Model.Load(stream, out var inputSchema);
		}

		var predEngine = mlContext.Model.CreatePredictionEngine<TicketData, TicketPrediction>(model);
		var result = predEngine.Predict(new TicketData { Title = _title, Description = _description });

		_result = $"Przewidywana kategoria: {result.PredictedCategory}";
	}
}