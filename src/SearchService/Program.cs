using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var db = await DB.InitAsync(
    "SearchDb",
    MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection"))
);
builder.Services.AddSingleton(db);

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(db, app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryForeverAsync(
            retryAttempt => TimeSpan.FromSeconds(3),
            onRetry: (outcome, retryAttempt) =>
            {
                Console.WriteLine($"Retry {retryAttempt} due to: {outcome.Exception?.Message}");
            }
        );
}