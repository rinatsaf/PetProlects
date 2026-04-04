using Application.DTOs.Payments;
using FluentValidation;

namespace Application.Validation;

public sealed class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("ReturnUrl must be a valid absolute URL.");
    }
}
