using Application.DTOs.Halls;
using FluentValidation;

namespace Application.Validation;

public sealed class UpdateHallRequestValidator : AbstractValidator<UpdateHallRequest>
{
    public UpdateHallRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.RowsCount).InclusiveBetween(1, 50);
        RuleFor(x => x.SeatsPerRow).InclusiveBetween(1, 50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}
