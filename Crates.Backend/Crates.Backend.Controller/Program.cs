using Microsoft.OpenApi.Models;
using Crates.Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });
});

#if DEBUG
builder.Environment.EnvironmentName = "Development";
#endif

builder.Services.AddHttpClient();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IDiscogsSearchService>(provider =>
{
    var httpClient = new HttpClient();
    string consumerKey = "eNABZZICxzejRrQxORSX";
    string consumerSecret = "NkcnxoVvGHkTkXNjOcInkdYacwRVABPE";

    string accessToken = "your_access_token";
    string accessTokenSecret = "your_access_token_secret";
    return new DiscogsSearchService(httpClient, consumerKey, consumerSecret, accessToken, accessTokenSecret);
});

builder.Services.AddSingleton<DiscogsOAuthService>(provider =>
{
    var httpClient = new HttpClient();
    string consumerKey = "eNABZZICxzejRrQxORSX";
    string consumerSecret = "NkcnxoVvGHkTkXNjOcInkdYacwRVABPE";
    string callbackUrl = "https://localhost:5001/api/oauth/callback"; // Update this to your actual callback URL
    return new DiscogsOAuthService(httpClient, consumerKey, consumerSecret, callbackUrl);
});


// Azure Blob Storage
builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("AzureBlobStorage connection string is not configured.");
    }
    return new BlobServiceClient(connectionString);
});

// Azure Computer Vision
builder.Services.AddSingleton(x =>
{
    var visionKey = builder.Configuration["AzureVision:Key"];
    var visionEndpoint = builder.Configuration["AzureVision:Endpoint"];

    if (string.IsNullOrEmpty(visionKey) || string.IsNullOrEmpty(visionEndpoint))
    {
        throw new InvalidOperationException("Azure Computer Vision key or endpoint is not configured.");
    }

    return new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
    {
        Endpoint = visionEndpoint
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name v1");
        c.RoutePrefix = string.Empty;  // Serve the Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();