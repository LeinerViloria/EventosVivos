using EventosVivos.Application.Features.Auth.Login;
using FluentValidation.TestHelper;

namespace EventosVivos.Application.Tests.Features.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Passes_for_valid_credentials()
    {
        _validator
            .TestValidate(new LoginCommand("admin@eventosvivos.dev", "Admin123*"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_the_email_is_not_an_email()
    {
        _validator
            .TestValidate(new LoginCommand("not-an-email", "Admin123*"))
            .ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorCode("AUTH_EMAIL_INVALID");
    }

    [Fact]
    public void Fails_when_the_password_is_empty()
    {
        _validator
            .TestValidate(new LoginCommand("admin@eventosvivos.dev", ""))
            .ShouldHaveValidationErrorFor(c => c.Password)
            .WithErrorCode("AUTH_PASSWORD_REQUIRED");
    }
}
