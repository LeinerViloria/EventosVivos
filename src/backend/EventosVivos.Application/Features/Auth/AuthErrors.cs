using EventosVivos.Domain.Common;

namespace EventosVivos.Application.Features.Auth;

public static class AuthErrors
{
    /// <summary>Wrong email or password. Deliberately generic so it does not reveal which one failed.</summary>
    public static readonly Error InvalidCredentials = new("AUTH_INVALID_CREDENTIALS");

    /// <summary>The email is already registered, so the account cannot be created again.</summary>
    public static readonly Error EmailAlreadyRegistered = new("AUTH_EMAIL_ALREADY_REGISTERED");
}
