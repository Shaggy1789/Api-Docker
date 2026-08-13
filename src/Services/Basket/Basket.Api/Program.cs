using Basket.Api.Data;
using Basket.Api.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using Marten;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Add services to the container.
builder.Services.AddCarter();
builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(assembly);
    conf.AddOpenBehavior(typeof(ValidationBehavior<,>));
    conf.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddMarten(opt =>
{
    opt.Connection(builder.Configuration.GetConnectionString("Database")!);
    opt.Schema.For<ShoppingCart>().Identity(x => x.Username);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
var redisConfig = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

ConfigurationOptions redisOptions;
if (redisConfig.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase) ||
    redisConfig.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(redisConfig);
    var userInfo = uri.UserInfo?.Split(':') ?? Array.Empty<string>();
    redisOptions = new ConfigurationOptions
    {
        EndPoints = { $"{uri.Host}:{(uri.Port > 0 ? uri.Port : 6379)}" },
        Password = userInfo.Length > 1 ? userInfo[1] : null,
        Ssl = uri.Scheme == "rediss",
        AbortOnConnectFail = false,
        ConnectRetry = 5,
        ConnectTimeout = 10000,
        SyncTimeout = 5000
    };
}
else
{
    redisOptions = ConfigurationOptions.Parse(redisConfig);
    redisOptions.AbortOnConnectFail = false;
    redisOptions.ConnectRetry = 5;
    redisOptions.ConnectTimeout = 10000;
    redisOptions.SyncTimeout = 5000;
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisOptions;
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.MapCarter();
app.MapHealthChecks("/healthz");
app.Run();
