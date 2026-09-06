namespace Collective.Api.Common.Security;

/// <summary>
/// Cabeceras de seguridad de PLAN_BACKEND.md 5.2. Veinte lineas propias en vez
/// de una dependencia (regla P2).
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var headers = context.Response.Headers;

        // Una API no sirve paginas: no necesita cargar nada.
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), microphone=(), payment=(), usb=()";
        headers["Cross-Origin-Resource-Policy"] = "same-site";

        headers.Remove("X-Powered-By");

        return _next(context);
    }
}
