using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using Application.Abstractions.Security;
using Microsoft.AspNetCore.Authentication;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cinema API",
                Version = "v1",
                Description = "Backend API for cinema management, ticket booking, payments and recommendations."
            });

            var apiXmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFilename);
            if (File.Exists(apiXmlPath))
            {
                options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
            }

            var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, "Application.xml");
            if (File.Exists(applicationXmlPath))
            {
                options.IncludeXmlComments(applicationXmlPath);
            }

            options.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Name = ".AspNetCore.Cookies",
                Description = "Cookie authentication. Sign in via /api/auth/login and send auth cookie."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "cookieAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = async ctx =>
                {
                    if (ctx.Principal?.Identity?.IsAuthenticated != true)
                        return;

                    var userIdClaim = ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!long.TryParse(userIdClaim, out var userId))
                        return;

                    var cache = ctx.HttpContext.RequestServices
                        .GetRequiredService<IUserActiveCacheService>();

                    if (!await cache.GetActiveAsync(userId))
                    {
                        ctx.RejectPrincipal();
                        await ctx.HttpContext.SignOutAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Customer", policy => policy.RequireRole("Customer", "Cashier", "Admin"));
            options.AddPolicy("Staff", policy => policy.RequireRole("Cashier", "Admin"));
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        return services;
    }
}
