using System.Net;
using System.Net.Http.Json;

namespace EventosVivos.Api.Tests;

public sealed class LoginEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record TokensResponse(string IdentityToken, string PermissionsToken);

    [Fact]
    public async Task Returns_both_tokens_for_the_seeded_admin()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "admin@eventosvivos.dev", password = "Admin123*" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<TokensResponse>();
        Assert.False(string.IsNullOrWhiteSpace(tokens!.IdentityToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.PermissionsToken));
    }

    [Fact]
    public async Task Returns_401_for_a_wrong_password()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "admin@eventosvivos.dev", password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
