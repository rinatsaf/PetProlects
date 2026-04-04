using Application.DTOs.Orders;
using FluentValidation;

namespace Application.Validation;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.SeatIds).NotEmpty();
        RuleForEach(x => x.SeatIds).GreaterThan(0);
        RuleFor(x => x.HoldId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}
