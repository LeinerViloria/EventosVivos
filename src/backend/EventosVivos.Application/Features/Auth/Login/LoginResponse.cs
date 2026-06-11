namespace EventosVivos.Application.Features.Auth.Login;

/// <summary>
/// The two tokens issued on sign-in. The identity token travels in every request; the
/// permissions token lives in the frontend only to show or hide UI.
/// </summary>
public sealed record LoginResponse(string IdentityToken, string PermissionsToken);
