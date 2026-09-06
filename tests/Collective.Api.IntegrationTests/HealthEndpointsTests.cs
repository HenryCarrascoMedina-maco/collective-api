using System.Net;

namespace Collective.Api.IntegrationTests;

public sealed class HealthEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_answer_200(string path)
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
