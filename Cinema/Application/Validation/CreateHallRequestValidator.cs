using Application.DTOs.Halls;
using FluentValidation;

namespace Application.Validation;

public sealed class CreateHallRequestValidator : AbstractValidator<CreateHallRequest>
{
    public CreateHallRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RowsCount).InclusiveBetween(1, 50);
        RuleFor(x => x.SeatsPerRow).InclusiveBetween(1, 50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}
