using FluentValidation;
using IdentityService.Application.Commands;

namespace IdentityService.Application.Validation;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).Must(x => x is "Customer" or "Seller" or "Admin")
            .WithMessage("Invalid role");
    }
}