using Microsoft.Extensions.Hosting;
using Serilog;

namespace Shared.Logging;

public static class LoggerExtensions
{
    public static IHostBuilder UseSharedLogging(this IHostBuilder builder)
    {
        builder.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console()
                .WriteTo.Seq(context.Configuration["Seq:Url"] ?? "http://localhost:5341");
        });

        return builder;
    }
}
