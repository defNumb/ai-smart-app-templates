using AI_RAG_Excelligence.Interfaces;
using AI_RAG_Excelligence.Services;
using DBTransferProject.AIServices;
using DBTransferProject.Components;
using DBTransferProject.Hubs;
using DBTransferProject.Services;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:8080", "http://localhost:8081");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Register HttpClient as a Singleton
builder.Services.AddSingleton(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri("https://api.kustomerapp.com") };
    httpClient.DefaultRequestHeaders.Add("accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY2NjFmYTFkNmFkMjRhMmUxMmM3ZGViNiIsInVzZXIiOiI2NjYxZmExZDkwYjEzMDRiNmIyN2U3Y2MiLCJvcmciOiI1ZDAyZTNjODcxMmQ0YzAwMWE2ZjhkZjIiLCJvcmdOYW1lIjoiZGlzY291bnRzY2hvb2xzdXBwbHkiLCJ1c2VyVHlwZSI6Im1hY2hpbmUiLCJwb2QiOiJwcm9kMSIsInJvbGVzIjpbIm9yZy51c2VyLmNvbnZlcnNhdGlvbi5yZWFkIiwib3JnLnVzZXIubWVzc2FnZS5yZWFkIl0sImV4cCI6MTcxODMwMTg1MywiYXVkIjoidXJuOmNvbnN1bWVyIiwiaXNzIjoidXJuOmFwaSIsInN1YiI6IjY2NjFmYTFkOTBiMTMwNGI2YjI3ZTdjYyJ9.Ipazd85fOHfVDyCqmLee1OpWoE0V8A7KrUIel_DxeXU");
    return httpClient;
});

// Register KustomerService
builder.Services.AddScoped<KustomerService>();

// Register RAG services
builder.Services.AddSingleton<IVectorStore, PineconeService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<PineconeService>>();
    return new PineconeService("592dfabf-9066-48e2-89b1-0e57b9545bab", "https://excelligence-training-data-32583iz.svc.aped-4627-b74a.pinecone.io", logger);
});
builder.Services.AddSingleton<IAugmentationService, AugmentationService>(provider =>
    new AugmentationService("sk-proj-EqLbkqkRasBapsJfHeXaT3BlbkFJXwdnHNd3bre1NwERq5Er"));
builder.Services.AddSingleton<EmbeddingsService>(provider =>
    new EmbeddingsService("sk-proj-EqLbkqkRasBapsJfHeXaT3BlbkFJXwdnHNd3bre1NwERq5Er"));
builder.Services.AddSingleton<IRetrievalService, RetrievalService>();

// Register dependent services
builder.Services.AddTransient<EntityExtractionAgent>();
builder.Services.AddTransient<ValidationAgent>();
builder.Services.AddTransient<AgentOrchestrator>();

// Register AI Service Agents
builder.Services.AddSingleton<OpenAIAPI>(sp => new OpenAIAPI("sk-proj-EqLbkqkRasBapsJfHeXaT3BlbkFJXwdnHNd3bre1NwERq5Er"));
builder.Services.AddTransient<CategorizationAgent>();
builder.Services.AddTransient<SentimentAnalysisAgent>();
builder.Services.AddTransient<EntityExtractionAgent>();
builder.Services.AddTransient<ImportantInformationExtractionAgent>();
builder.Services.AddTransient<RecommendedActionAgent>();
builder.Services.AddTransient<CarrierTrackingAgent>();
builder.Services.AddTransient<TrackingInfoAgent>();
builder.Services.AddTransient<UrgencyDetectionAgent>();
builder.Services.AddTransient<ValidationAgent>();
builder.Services.AddTransient<CostTracker>();
builder.Services.AddTransient<ActionSelectionAgent>();
builder.Services.AddTransient<FilterAgent>();
builder.Services.AddTransient<MockJDAService>();

// Register Agent Orchestrator
builder.Services.AddTransient<AgentOrchestrator>();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = true;
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Ensure antiforgery token validation is not enforced globally
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

// Map SignalR hub
app.MapHub<KustomerHub>("/kustomerHub");

app.Run();

