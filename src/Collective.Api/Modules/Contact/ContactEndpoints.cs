using Collective.Api.Common.Results;
using Collective.Api.Common.Security;
using Collective.Api.Common.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Collective.Api.Modules.Contact;

/// <summary>
/// Superficie HTTP del modulo. Valida, llama al servicio y traduce el resultado.
/// Cero logica de negocio (regla A1).
/// </summary>
public static class ContactEndpoints
{
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var group = builder.MapGroup("/api/v1/contact")
            .WithTags("Contact")
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);

        group.MapPost(string.Empty, SubmitAsync)
            .WithName("SubmitContactRequest")
            .WithSummary("Envia una solicitud de contacto")
            .Produces<ContactSubmissionResult>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return builder;
    }

    private static async Task<IResult> SubmitAsync(
        SubmitContactRequest request,
        ContactService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var errors = DataAnnotationsValidator.Collect(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var context = new ContactRequestContext(
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString());

        var result = await service.SubmitAsync(request, context, cancellationToken)
            .ConfigureAwait(false);

        return result.IsSuccess
            ? TypedResults.Accepted((string?)null, result.Value)
            : Problem(result.Error);
    }

    private static ProblemHttpResult Problem(Error? error) => TypedResults.Problem(
        title: "The request could not be completed",
        statusCode: error?.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        });
}
