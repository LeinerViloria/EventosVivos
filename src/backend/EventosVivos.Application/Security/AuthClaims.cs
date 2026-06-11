namespace EventosVivos.Application.Security;

/// <summary>
/// Custom JWT claim names shared between the token service that writes them and the API that
/// validates the tokens and reads them.
/// </summary>
public static class AuthClaims
{
    public const string SessionId = "sid";
    public const string Permission = "perm";
    public const string Name = "name";
    public const string Email = "email";

    /// <summary>Short role claim used in the frontend-only permissions token.</summary>
    public const string Role = "role";
}
