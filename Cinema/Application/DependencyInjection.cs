using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.Mapping;
using Application.Services;
using Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MoviesProfile).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateMovieRequestValidator>();

        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}
