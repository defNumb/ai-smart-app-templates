using DBTransferProject.Components;
using DBTransferProject.Services;
using OpenAI_API;
using DBTransferProject.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Azure.AI.TextAnalytics;
using Azure;
using DBTransferProject.AIServices;
using System.Net.Http;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:8080", "http://localhost:8081");
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); 
// Add Kustomer API token to configuration
//var kustomerApiToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY2NDY1YTE4NGViNDhkNzlhNzJmNWIyYSIsInVzZXIiOiI2NjQ2NWExODAzNDFiMTA0MWQzNmI3NDgiLCJvcmciOiI1ZDAyZTNjODcxMmQ0YzAwMWE2ZjhkZjIiLCJvcmdOYW1lIjoiZGlzY291bnRzY2hvb2xzdXBwbHkiLCJ1c2VyVHlwZSI6Im1hY2hpbmUiLCJwb2QiOiJwcm9kMSIsInJvbGVzIjpbIm9yZy51c2VyLmNvbnZlcnNhdGlvbi5yZWFkIiwib3JnLnVzZXIubWVzc2FnZS5yZWFkIl0sImV4cCI6MTcxNjQ5MTQxNSwiYXVkIjoidXJuOmNvbnN1bWVyIiwiaXNzIjoidXJuOmFwaSIsInN1YiI6IjY2NDY1YTE4MDM0MWIxMDQxZDM2Yjc0OCJ9.xnVnOuZC89Xnp2XMb7v1xByhQbWjo0NE7AInDUwiZnw";

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
