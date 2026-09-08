using Collective.Api.Common.Validation;
using Collective.Api.Modules.Contact;

namespace Collective.Api.UnitTests.Common;

public sealed class DataAnnotationsValidatorTests
{
    [Fact]
    public void A_valid_request_produces_no_errors()
    {
        var request = new SubmitContactRequest
        {
            Name = "Ada",
            Email = "ada@example.com",
            Reason = "consulting",
            Message = "Un mensaje suficientemente largo para pasar el minimo.",
        };

        Assert.Empty(DataAnnotationsValidator.Collect(request));
    }

    [Fact]
    public void Errors_are_grouped_by_field_in_camel_case()
    {
        var request = new SubmitContactRequest
        {
            Name = string.Empty,
            Email = "roto",
            Reason = "consulting",
            Message = "corto",
        };

        var errors = DataAnnotationsValidator.Collect(request);

        Assert.Contains("name", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("email", errors.Keys, StringComparer.Ordinal);
        Assert.Contains("message", errors.Keys, StringComparer.Ordinal);
    }
}
