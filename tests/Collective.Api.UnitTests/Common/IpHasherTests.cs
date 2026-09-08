using Collective.Api.Common.Security;
using Microsoft.Extensions.Configuration;

namespace Collective.Api.UnitTests.Common;

public sealed class IpHasherTests
{
    private static IpHasher Create(string salt) => new(
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Security:IpHashSalt"] = salt })
            .Build());

    [Fact]
    public void The_address_never_appears_in_the_result()
    {
        var hash = Create("s3cr3t").Hash("203.0.113.42");

        Assert.NotNull(hash);
        Assert.DoesNotContain("203.0.113", hash, StringComparison.Ordinal);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void The_same_address_gives_the_same_hash()
    {
        var hasher = Create("s3cr3t");

        Assert.Equal(hasher.Hash("203.0.113.42"), hasher.Hash("203.0.113.42"));
    }

    [Fact]
    public void A_different_salt_gives_a_different_hash()
    {
        Assert.NotEqual(Create("one").Hash("203.0.113.42"), Create("two").Hash("203.0.113.42"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_address_gives_nothing(string? address)
    {
        Assert.Null(Create("s3cr3t").Hash(address));
    }
}
