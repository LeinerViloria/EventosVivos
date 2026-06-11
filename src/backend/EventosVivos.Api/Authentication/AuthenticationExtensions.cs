using System.Security.Claims;
using System.Text;
using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Security;
using EventosVivos.Domain.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace EventosVivos.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var signingKey = configuration["JWT_SIGNING_KEY"]
            ?? throw new InvalidOperationException("JWT_SIGNING_KEY is required.");
        var issuer = configuration["JWT_ISSUER"] ?? "eventosvivos";
        var audience = configuration["JWT_AUDIENCE"] ?? "eventosvivos";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role,
                };
                options.Events = new JwtBearerEvents { OnTokenValidated = OnTokenValidatedAsync };
            });

        services.AddAuthorization(options =>
        {
            // Behind login: every endpoint needs an authenticated user unless it opts out.
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(AuthClaims.Permission, permission));
            }
        });

        return services;
    }

    /// <summary>
    /// Confirms the session still exists in Redis (revocation control) and attaches the role's
    /// effective permissions from the catalog, so policies can authorize by permission.
    /// </summary>
    private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        var services = context.HttpContext.RequestServices;
        var cancellationToken = context.HttpContext.RequestAborted;

        var sessions = services.GetRequiredService<ISessionStore>();
        var sessionId = context.Principal?.FindFirst(AuthClaims.SessionId)?.Value;
        if (!Guid.TryParse(sessionId, out var session)
            || await sessions.GetAsync(session, cancellationToken) is null)
        {
            context.Fail("The session is no longer valid.");
            return;
        }

        if (context.Principal?.Identity is ClaimsIdentity identity
            && Enum.TryParse<UserRole>(identity.FindFirst(ClaimTypes.Role)?.Value, out var role))
        {
            var permissions = services.GetRequiredService<IPermissionStore>();
            foreach (var permission in await permissions.GetPermissionsAsync(role, cancellationToken))
            {
                identity.AddClaim(new Claim(AuthClaims.Permission, permission));
            }
        }
    }
}
