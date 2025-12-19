using FluentValidation;
using TouristRoutes.Models.DTOs.Request;

namespace TouristRoutes.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is invalid");
        
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters");

        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must have at least 8 characters")
            .Matches("[A-Z]")
            .WithMessage("Password must have at least 1 uppercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain a digit");
    }
}