using Collective.Api.Common.Observability;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Collective.Api.Common.Errors;

/// <summary>
/// Ultimo recorte antes de la respuesta. Registra el detalle completo y devuelve
/// al cliente un mensaje generico con la correlacion: ni traza, ni nombre de
/// tabla, ni mensaje de excepcion (regla S4).
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var correlationId = httpContext.GetCorrelationId();

        _logger.LogError(
            exception,
            "Unhandled exception. CorrelationId: {CorrelationId}. Path: {Path}",
            correlationId,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
                Detail = "The request could not be completed. Quote the correlation id if you contact support.",
            },
        }).ConfigureAwait(false);
    }
}
