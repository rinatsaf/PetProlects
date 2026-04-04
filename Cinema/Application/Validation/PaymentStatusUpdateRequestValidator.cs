using Application.DTOs.Payments;
using FluentValidation;

namespace Application.Validation;

public sealed class PaymentStatusUpdateRequestValidator : AbstractValidator<PaymentStatusUpdateRequest>
{
    public PaymentStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
