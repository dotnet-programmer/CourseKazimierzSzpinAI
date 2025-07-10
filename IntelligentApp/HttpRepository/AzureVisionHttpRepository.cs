using System.Text.Json;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Models.AzureVision;

namespace IntelligentApp.HttpRepository;

public class AzureVisionHttpRepository(HttpClient httpClient) : IAzureVisionHttpRepository
{
	// na podstawie przesłanej tablicy bajtów zwróci informacje o obrazie
	public async Task<ImageAnalysisResponse?> GetImageInfoAsync(byte[] image)
	{
		// parametr features określa co ma zostać zrobione z obrazem
		// caption - opis obrazu
		// tags - lista tagów
		// read - OCR - odczytanie tekstu z obrazu
		// endpoint przeniesiony do konfiguracji w appsettings.json
		//var endpoint = "computervision/imageanalysis:analyze?api-version=2024-02-01&features=caption,tags,read";

		// zawartość żądania to surowe dane binarne
		using var content = new ByteArrayContent(image);
		content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

		var response = await httpClient.PostAsync("", content);
		response.EnsureSuccessStatusCode();

		var responseString = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<ImageAnalysisResponse>(responseString);
	}
}