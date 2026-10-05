using EventBus;
using ProductService.Api.Endpoints;
using ProductService.Application;
using ProductService.Infrastructure;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedLogging();

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("MongoDb")!,
    builder.Configuration["DatabaseName"] ?? "ProductDb");
builder.Services.AddMessageBus(
    builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapProductEndpoints();

app.Run();