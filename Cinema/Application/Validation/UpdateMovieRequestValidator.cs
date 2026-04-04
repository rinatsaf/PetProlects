using Application.DTOs.Movies;
using FluentValidation;

namespace Application.Validation;

public sealed class UpdateMovieRequestValidator : AbstractValidator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.AgeRating)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country cannot be empty")
            .MaximumLength(50);
        
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(60, 200)
            .WithMessage("Duration minutes must be between 1 and 200");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty()
            .GreaterThan(new DateOnly(1900, 1, 1))
            .WithMessage("ReleaseDate must be provided and after 1900-01-01")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("ReleaseDate cannot be in the future");
    }
}
