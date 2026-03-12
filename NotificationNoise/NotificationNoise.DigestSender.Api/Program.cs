using NotificationNoise.DigestSender.Application;
using NotificationNoise.DigestSender.Infrastructure.Persistence;
using NotificationNoise.DigestSender.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDigestSenderInfrastructure(builder.Configuration);
builder.Services.AddScoped<DigestService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DigestDbContext>();
    db.Database.Migrate();
}
app.MapControllers();

app.Run();
