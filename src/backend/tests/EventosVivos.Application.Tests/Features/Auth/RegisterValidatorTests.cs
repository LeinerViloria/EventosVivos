using EventosVivos.Application.Features.Auth.Register;
using FluentValidation.TestHelper;

namespace EventosVivos.Application.Tests.Features.Auth;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Passes_for_a_valid_registration()
    {
        _validator
            .TestValidate(new RegisterCommand("Nueva Persona", "new@example.com", "Password1"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_the_password_is_too_short()
    {
        _validator
            .TestValidate(new RegisterCommand("Nueva Persona", "new@example.com", "short"))
            .ShouldHaveValidationErrorFor(c => c.Password)
            .WithErrorCode("AUTH_PASSWORD_TOO_SHORT");
    }

    [Fact]
    public void Fails_when_the_name_is_empty()
    {
        _validator
            .TestValidate(new RegisterCommand("", "new@example.com", "Password1"))
            .ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorCode("AUTH_NAME_REQUIRED");
    }
}
