using System.Net;
using System.Net.Http.Json;

namespace EventosVivos.Api.Tests;

public sealed class RegisterEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record TokensResponse(string IdentityToken, string PermissionsToken);

    [Fact]
    public async Task Registers_a_new_user_and_returns_tokens()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { name = "Persona Nueva", email = "persona.nueva@example.com", password = "Password1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<TokensResponse>();
        Assert.False(string.IsNullOrWhiteSpace(tokens!.IdentityToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.PermissionsToken));
    }

    [Fact]
    public async Task Rejects_an_email_that_is_already_registered()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { name = "Admin Duplicado", email = "admin@eventosvivos.dev", password = "Password1" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
