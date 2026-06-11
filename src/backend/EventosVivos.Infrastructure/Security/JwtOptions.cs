namespace EventosVivos.Infrastructure.Security;

/// <summary>JWT settings sourced from the environment (.env), never hardcoded.</summary>
public sealed class JwtOptions
{
    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public int IdentityTokenMinutes { get; init; }

    public int PermissionsTokenMinutes { get; init; }

    public static JwtOptions Build(Func<string, string?> getValue) =>
        new()
        {
            Issuer = getValue("JWT_ISSUER") ?? "eventosvivos",
            Audience = getValue("JWT_AUDIENCE") ?? "eventosvivos",
            SigningKey = getValue("JWT_SIGNING_KEY")
                ?? throw new InvalidOperationException("JWT_SIGNING_KEY is required."),
            IdentityTokenMinutes = int.TryParse(getValue("JWT_IDENTITY_TOKEN_MINUTES"), out var i) ? i : 15,
            PermissionsTokenMinutes =
                int.TryParse(getValue("JWT_PERMISSIONS_TOKEN_MINUTES"), out var p) ? p : 15,
        };
}
