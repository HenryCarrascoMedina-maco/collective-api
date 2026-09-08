using System.Net;
using System.Net.Http.Json;
using Collective.Api.Modules.Contact;
using Microsoft.EntityFrameworkCore;

namespace Collective.Api.IntegrationTests;

public sealed class ContactEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly Uri Endpoint = new("/api/v1/contact", UriKind.Relative);
    private readonly ApiFactory _factory = factory;

    private static SubmitContactRequest Valid() => new()
    {
        Name = "Ada Lovelace",
        Email = "Ada@Example.COM",
        Company = "Analytical Engines",
        Reason = "custom-dev",
        Message = "Necesitamos automatizar la firma de nuestros instaladores.",
        SourcePage = "/contacto",
    };

    [Fact]
    public async Task A_valid_submission_is_accepted_and_stored()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync(Endpoint, Valid() with { });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var stored = await _factory.WithDbAsync(db => db.Set<ContactRequest>()
            .SingleOrDefaultAsync(request => request.Company == "Analytical Engines"));

        Assert.NotNull(stored);
        Assert.Equal("Ada Lovelace", stored.Name);
        Assert.Equal(ContactReason.CustomDev, stored.Reason);
        Assert.Equal(ContactRequestStatus.New, stored.Status);
    }

    [Fact]
    public async Task The_email_is_normalised_to_lowercase()
    {
        using var client = _factory.CreateClient();
        var payload = Valid() with { Company = "Normalisation Ltd" };

        using var response = await client.PostAsJsonAsync(Endpoint, payload);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var stored = await _factory.WithDbAsync(db => db.Set<ContactRequest>()
            .SingleAsync(request => request.Company == "Normalisation Ltd"));

        Assert.Equal("ada@example.com", stored.Email);
    }

    [Fact]
    public async Task The_ip_is_never_stored_in_clear()
    {
        using var client = _factory.CreateClient();
        var payload = Valid() with { Company = "Privacy Co" };

        await client.PostAsJsonAsync(Endpoint, payload);

        var stored = await _factory.WithDbAsync(db => db.Set<ContactRequest>()
            .SingleAsync(request => request.Company == "Privacy Co"));

        // O no hay IP, o es un hash de 64 caracteres hexadecimales.
        if (stored.IpHash is not null)
        {
            Assert.Equal(64, stored.IpHash.Length);
            Assert.DoesNotContain(".", stored.IpHash, StringComparison.Ordinal);
            Assert.DoesNotContain(":", stored.IpHash, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("name", "")]
    [InlineData("email", "no-es-un-correo")]
    [InlineData("message", "corto")]
    [InlineData("reason", "inventado")]
    public async Task Invalid_input_is_rejected_with_field_errors(string field, string value)
    {
        using var client = _factory.CreateClient();
        var payload = field switch
        {
            "name" => Valid() with { Name = value },
            "email" => Valid() with { Email = value },
            "message" => Valid() with { Message = value },
            _ => Valid() with { Reason = value },
        };

        using var response = await client.PostAsJsonAsync(Endpoint, payload);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(field, body, StringComparison.OrdinalIgnoreCase);
        // Ni siquiera al fallar se filtra nada interno.
        Assert.DoesNotContain("Collective.Api", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_message_over_the_limit_is_rejected()
    {
        using var client = _factory.CreateClient();
        var payload = Valid() with { Message = new string('a', 4001) };

        using var response = await client.PostAsJsonAsync(Endpoint, payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_honeypot_swallows_bots_without_storing_anything()
    {
        using var client = _factory.CreateClient();
        var payload = Valid() with { Company = "Bot Inc", Website = "http://spam.example" };

        using var response = await client.PostAsJsonAsync(Endpoint, payload);

        // Se responde con exito para no ensenarle al bot que le hemos visto...
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        // ...pero no se guarda nada.
        var stored = await _factory.WithDbAsync(db => db.Set<ContactRequest>()
            .AnyAsync(request => request.Company == "Bot Inc"));

        Assert.False(stored);
    }

    [Fact]
    public async Task The_endpoint_is_rate_limited()
    {
        // Fabrica propia: el limitador cuenta por proceso y no debe arrastrar
        // los envios de los demas tests.
        using var factory = ApiFactory.WithPermitLimit(3);
        using var client = factory.CreateClient();

        var codes = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
        {
            using var response = await client.PostAsJsonAsync(
                Endpoint, Valid() with { Company = $"Flood {i}" });
            codes.Add(response.StatusCode);
        }

        Assert.Contains(HttpStatusCode.TooManyRequests, codes);
    }
}
