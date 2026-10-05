using Api.Extensions;
using Api.Hangfire;
using Application;
using Hangfire;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Filters;
using System.Net;
using System.Threading.RateLimiting;
using Api.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty("BusinessLog"))
        .WriteTo.File(
            path: "logs/business-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Local reverse-proxy
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    // Docker bridge networks
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
    options.ForwardLimit = 1;
});

builder.Services.AddCinemaHangfire(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddApiDocumentation()
    .AddApiAuthentication();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error-development");
    app.UseHsts();
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>("/error");
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseApiPipeline();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AdminOnlyDashboardAuthorizationFilter()]
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    RecurringJobsRegistrar.Register(recurringJobs);
}

app.Run();
