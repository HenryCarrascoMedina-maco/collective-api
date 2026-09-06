using System.Net;

namespace Collective.Api.IntegrationTests;

public sealed class SecurityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Unknown_route_returns_problem_details_without_internals()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/no-existe", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Ni traza, ni rutas de archivo, ni nombres de tipos internos.
        Assert.DoesNotContain("Collective.Api", body, StringComparison.Ordinal);
        Assert.DoesNotContain("at ", body, StringComparison.Ordinal);
        Assert.DoesNotContain("D:\\", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Responses_carry_the_security_headers()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Every_response_carries_a_correlation_id()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        var correlationId = response.Headers.GetValues("X-Correlation-Id").Single();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task A_hostile_correlation_id_is_discarded()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/health/live", UriKind.Relative));
        request.Headers.Add("X-Correlation-Id", "<script>alert(1)</script>");

        using var response = await client.SendAsync(request);

        var correlationId = response.Headers.GetValues("X-Correlation-Id").Single();
        Assert.DoesNotContain("<script>", correlationId, StringComparison.Ordinal);
    }
}
