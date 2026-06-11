using FluentValidation;

namespace EventosVivos.Application.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().WithErrorCode("AUTH_EMAIL_INVALID");
        RuleFor(c => c.Password).NotEmpty().WithErrorCode("AUTH_PASSWORD_REQUIRED");
    }
}
