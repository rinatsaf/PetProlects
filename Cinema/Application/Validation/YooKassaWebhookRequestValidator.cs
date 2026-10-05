using Application.DTOs.Payments;
using FluentValidation;

namespace Application.Validation;

public sealed class YooKassaWebhookRequestValidator : AbstractValidator<YooKassaWebhookRequest>
{
    private static readonly string[] ValidEvents =
    [
        "payment.succeeded",
        "payment.canceled",
        "payment.waiting_for_capture",
        "payment.pending"
    ];

    public YooKassaWebhookRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Equal("notification");

        RuleFor(x => x.Event)
            .NotEmpty()
            .Must(e => ValidEvents.Contains(e))
            .WithMessage("Unknown webhook event: {PropertyValue}");

        RuleFor(x => x.Payment)
            .NotNull();

        When(x => x.Payment is not null, () =>
        {
            RuleFor(x => x.Payment!.Id)
                .NotEmpty();

            RuleFor(x => x.Payment!.Status)
                .NotEmpty();
        });
    }
}
