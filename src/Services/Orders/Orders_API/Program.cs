using Orders_API.Data;
using Orders_API.Endpoints;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using MediatR;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Carter
builder.Services.AddCarter();

// MediatR
builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(typeof(Program).Assembly);
    conf.AddOpenBehavior(typeof(ValidationBehavior<,>));
    conf.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Repository
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

// HTTP Client
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("BasketApi", client =>
{
    var baseUrl = builder.Configuration["BasketApi:BaseUrl"]
        ?? builder.Configuration["BasketApi__BaseUrl"]
        ?? "http://localhost:6001";
    client.BaseAddress = new Uri(baseUrl);
});

// Health Checks
builder.Services.AddHealthChecks();

// MongoDB
var mongoConnectionString =
    builder.Configuration["MongoDb:ConnectionString"];

var mongoDatabaseName =
    builder.Configuration["MongoDb:DatabaseName"];

if (string.IsNullOrWhiteSpace(mongoConnectionString))
{
    throw new InvalidOperationException(
        "MongoDb__ConnectionString is missing.");
}

if (string.IsNullOrWhiteSpace(mongoDatabaseName))
{
    throw new InvalidOperationException(
        "MongoDb__DatabaseName is missing.");
}

// IMPORTANTE: registrar como IMongoClient
builder.Services.AddSingleton<IMongoClient>(
    _ => new MongoClient(mongoConnectionString)
);

// Exception handling
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// HTTP pipeline
app.UseExceptionHandler();

app.MapCarter();

app.MapHealthChecks("/healthz");

app.Run();