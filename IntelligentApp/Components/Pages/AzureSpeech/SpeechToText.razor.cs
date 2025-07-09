using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace IntelligentApp.Components.Pages.AzureSpeech;

public partial class SpeechToText
{
	private bool _isLoading = false;
	private byte[]? _selectedAudio;
	private string? _transcript;

	[Inject]
	protected IAzureSpeechHttpRepository AzureSpeechHttpRepository { get; set; } = default!;

	[Inject]
	protected IFileReader FileReader { get; set; } = default!;

	private async Task OnAudioSelectedAsync(InputFileChangeEventArgs e)
	{
		try
		{
			_selectedAudio = await FileReader.ReadInputAsBytesAsync(e.File);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd podczas wczytywania pliku: {ex.Message}.");
			throw;
		}
	}

	private async Task RecognizeSpeechAsync()
	{
		_isLoading = true;
		_transcript = null;

		try
		{
			if (_selectedAudio != null)
			{
				_transcript = await AzureSpeechHttpRepository.GetTextAsync(_selectedAudio);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Błąd: {ex.Message}.");
			throw;
		}
		finally
		{
			_isLoading = false;
		}
	}
}