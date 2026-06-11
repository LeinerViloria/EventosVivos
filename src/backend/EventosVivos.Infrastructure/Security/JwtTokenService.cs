using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace EventosVivos.Infrastructure.Security;

internal sealed class JwtTokenService(JwtOptions options, IClock clock) : ITokenService
{
    /// <summary>Custom claim names shared with the API's token validation and the frontend.</summary>
    public const string SessionClaim = "sid";
    public const string PermissionClaim = "perm";
    public const string NameClaim = "name";

    public TimeSpan IdentityTokenLifetime => TimeSpan.FromMinutes(options.IdentityTokenMinutes);

    public string CreateIdentityToken(Guid userId, UserRole role, Guid sessionId)
    {
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Role, role.ToString()),
            new(SessionClaim, sessionId.ToString()),
        ];

        return Write(claims, options.IdentityTokenMinutes);
    }

    public string CreatePermissionsToken(UserRole role, string name, IReadOnlyList<string> permissions)
    {
        List<Claim> claims = [new(ClaimTypes.Role, role.ToString()), new(NameClaim, name)];
        claims.AddRange(permissions.Select(permission => new Claim(PermissionClaim, permission)));

        return Write(claims, options.PermissionsTokenMinutes);
    }

    private string Write(IEnumerable<Claim> claims, int minutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = clock.UtcNow.UtcDateTime;

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
