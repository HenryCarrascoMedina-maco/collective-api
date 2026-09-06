using Collective.Api.Common.Results;

namespace Collective.Api.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_without_value_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Success_with_value_exposes_the_value()
    {
        var result = Result.Success("collective");

        Assert.True(result.IsSuccess);
        Assert.Equal("collective", result.Value);
    }

    [Fact]
    public void Failure_carries_the_error()
    {
        var error = Error.Validation("email.invalid", "Email is not valid.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Reading_the_value_of_a_failure_throws()
    {
        var result = Result.Failure<string>(Error.NotFound("x.missing", "Not found."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    public void Error_factories_set_the_type(ErrorType expected)
    {
        var error = expected switch
        {
            ErrorType.Validation => Error.Validation("c", "m"),
            ErrorType.NotFound => Error.NotFound("c", "m"),
            _ => Error.Conflict("c", "m"),
        };

        Assert.Equal(expected, error.Type);
    }
}
