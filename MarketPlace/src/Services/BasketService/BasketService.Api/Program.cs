using BasketService.Api.Endpoints;
using BasketService.Application;
using BasketService.Infrastructure;
using EventBus;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedLogging();

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");
builder.Services.AddMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.AddBasketEndpoints();

app.Run();