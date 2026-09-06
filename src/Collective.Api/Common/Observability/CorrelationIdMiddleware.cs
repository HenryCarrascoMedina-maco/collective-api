namespace Collective.Api.Common.Observability;

/// <summary>
/// Asigna un identificador de correlacion a cada peticion, lo devuelve en la
/// respuesta y lo anade al ambito de log. Permite que alguien diga "me fallo,
/// el codigo era X" y encontrarlo sin exponer nada interno.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";
    private const string ItemKey = "CorrelationId";
    private const int MaxLength = 64;

    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ReadIncoming(context) ?? context.TraceIdentifier;
        context.Items[ItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { [ItemKey] = correlationId }))
        {
            await _next(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Acepta un identificador entrante solo si es corto y alfanumerico: viene
    /// de fuera y acaba en los logs, asi que se valida por lista blanca.
    /// </summary>
    private static string? ReadIncoming(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return null;
        }

        var candidate = values.ToString();
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > MaxLength)
        {
            return null;
        }

        return candidate.All(IsAllowed) ? candidate : null;

        static bool IsAllowed(char character) =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or ':';
    }
}

public static class CorrelationIdExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue("CorrelationId", out var value) && value is string id
            ? id
            : context.TraceIdentifier;
    }
}
