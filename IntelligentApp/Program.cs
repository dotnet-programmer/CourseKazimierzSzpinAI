using System.Net.Http.Headers;
using IntelligentApp.Components;
using IntelligentApp.HttpRepository;
using IntelligentApp.HttpRepository.Interfaces;
using IntelligentApp.Services;
using IntelligentApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
var openAiEndpoint = builder.Configuration["OpenAI:Endpoint"];
var azureApiKey = builder.Configuration["AzureAI:ApiKey"];
var azureEndpoint = builder.Configuration["AzureAI:Endpoint"];

builder.Services.AddHttpClient("OpenAI", client =>
{
	client.BaseAddress = new Uri(openAiEndpoint);
	// w ka¿dym requeœcie w headerze bêdie przesy³any ten klucz
	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
});
builder.Services.AddHttpClient("AzureAI", client =>
{
	client.BaseAddress = new Uri(azureEndpoint);
	client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApiKey);
});

// dodatkowa konfiguracja umo¿liwiaj¹ca korzystanie w repozytoriach z HttpClient bezpoœrednio
builder.Services.AddScoped<IOpenAiHttpRepository, OpenAiHttpRepository>(sp =>
{
	var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
	var httpClient = httpClientFactory.CreateClient("OpenAI");
	return new OpenAiHttpRepository(httpClient);
});
builder.Services.AddScoped<IAzureAiHttpRepository, AzureAiHttpRepository>(sp =>
{
	var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
	var httpClient = httpClientFactory.CreateClient("AzureAI");
	return new AzureAiHttpRepository(httpClient);
});

builder.Services.AddScoped<IFileReader, FileReaderService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Zwiêkszenie dopuszczalnego rozmiaru komunikatów w blazor server, potrzebne do przesy³ania wiêkszych plików np. audio
builder.Services
	.AddServerSideBlazor()
	.AddHubOptions(x =>
	{
		x.MaximumReceiveMessageSize = 10 * 1024 * 1024;//10MB
		x.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
	})
	.AddCircuitOptions(x =>
	{
		x.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5);
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
