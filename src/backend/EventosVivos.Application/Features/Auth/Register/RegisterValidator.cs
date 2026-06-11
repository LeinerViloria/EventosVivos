using FluentValidation;

namespace EventosVivos.Application.Features.Auth.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithErrorCode("AUTH_NAME_REQUIRED")
            .MaximumLength(120)
            .WithErrorCode("AUTH_NAME_REQUIRED");

        RuleFor(c => c.Email)
            .NotEmpty()
            .WithErrorCode("AUTH_EMAIL_INVALID")
            .EmailAddress()
            .WithErrorCode("AUTH_EMAIL_INVALID");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithErrorCode("AUTH_PASSWORD_TOO_SHORT")
            .MinimumLength(8)
            .WithErrorCode("AUTH_PASSWORD_TOO_SHORT");
    }
}
