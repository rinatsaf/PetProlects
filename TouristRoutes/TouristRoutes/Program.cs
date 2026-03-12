using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TouristRoutes.Data;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.Entity;
using TouristRoutes.Repositories;
using TouristRoutes.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using TouristRoutes.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// database postgresSQL
builder.Services.AddDbContext<RoutesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// cache service redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"]
        ?? throw new InvalidOperationException("Redis:ConnectionString is missing");
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "tourist:";
});

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddScoped<IEmailService, EmailService>();

// Authentification cokkie
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.Cookie.Name = "tourist_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(3);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

// Authorization for roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));
    
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});

// for cache
builder.Services
    .AddOptions<RateLimiterOptions>()
    .Bind(builder.Configuration.GetSection("RateLimiter"))
    .Validate(o => o.MaxAttempts > 0 && o.BanSeconds > 0, "RateLimiter values must be positive")
    .ValidateOnStart();

// rabbitmq options
builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMQ"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.HostName), "RabbitMQ:HostName is missing")
    .ValidateOnStart();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ILoginRateLimiter, LoginRateLimiter>();

// for data
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IPoiRepository, PoiRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();

// HashPassword
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Mapper for entity Services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Entity services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IAudioStorageService, AudioStorageService>();
builder.Services.AddScoped<IRoutePlanner, RoutePlanner>();
builder.Services.AddScoped<IAudioGenerationService, AudioGenerationService>();


builder.Services.AddControllers();

// cors for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
    {
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// Ensure database is up to date
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoutesDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseExceptionHandler("/error");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public class RateLimiterOptions
{
    public int MaxAttempts { get; set; }
    public int BanSeconds { get; set; }
}

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string QueueName { get; set; } = "tourist.jobs";
}
