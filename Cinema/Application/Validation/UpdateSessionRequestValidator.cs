using Application.DTOs.Sessions;
using FluentValidation;

namespace Application.Validation;

public sealed class UpdateSessionRequestValidator : AbstractValidator<UpdateSessionRequest>
{
    public UpdateSessionRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0);
        RuleFor(x => x.HallId).GreaterThan(0);
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be after StartTime");
    }
}
