using Application.DTOs.Movies;
using FluentValidation;

namespace Application.Validation;

public sealed class MovieSearchRequestValidator : AbstractValidator<MovieSearchRequest>
{
    public MovieSearchRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Country)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.AgeRating)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.AgeRating));

        RuleFor(x => x.Genre)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Genre));

        RuleFor(x => x.MinDurationMinutes)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinDurationMinutes.HasValue);

        RuleFor(x => x.MaxDurationMinutes)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MaxDurationMinutes.HasValue);

        RuleFor(x => x.MinPopularityScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPopularityScore.HasValue);

        RuleFor(x => x.MaxPopularityScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPopularityScore.HasValue);

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x => !x.ReleaseDateFrom.HasValue || !x.ReleaseDateTo.HasValue || x.ReleaseDateFrom <= x.ReleaseDateTo)
            .WithMessage("ReleaseDateFrom cannot be greater than ReleaseDateTo.");

        RuleFor(x => x)
            .Must(x => !x.MinDurationMinutes.HasValue || !x.MaxDurationMinutes.HasValue || x.MinDurationMinutes <= x.MaxDurationMinutes)
            .WithMessage("MinDurationMinutes cannot be greater than MaxDurationMinutes.");

        RuleFor(x => x)
            .Must(x => !x.MinPopularityScore.HasValue || !x.MaxPopularityScore.HasValue || x.MinPopularityScore <= x.MaxPopularityScore)
            .WithMessage("MinPopularityScore cannot be greater than MaxPopularityScore.");
    }
}
