using Application.DTOs.Orders;
using FluentValidation;

namespace Application.Validation;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        When(x => x.UserId.HasValue, () =>
        {
            RuleFor(x => x.UserId!.Value).GreaterThan(0);
        });
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.SeatIds).NotEmpty();
        RuleForEach(x => x.SeatIds).GreaterThan(0);
    }
}
