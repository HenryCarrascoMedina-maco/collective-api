using System.Diagnostics.CodeAnalysis;
using Collective.Api.Common.Configuration;
using Collective.Api.Common.Errors;
using Collective.Api.Common.Observability;
using Collective.Api.Common.Security;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Registro estructurado en JSON. Sin dependencias externas: la plataforma ya lo
// trae (regla P2). La correlacion la aporta CorrelationIdMiddleware.
// ---------------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
});

// ---------------------------------------------------------------------------
// Kestrel: limites y superficie. PLAN_BACKEND.md 5.2 y 5.3.
// ---------------------------------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    // No se regala la version del servidor.
    options.AddServerHeader = false;

    // Un mensaje legitimo del formulario no llega a 64 KB.
    options.Limits.MaxRequestBodySize = RequestLimits.MaxBodyBytes;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20);
});

// ---------------------------------------------------------------------------
// Configuracion tipada
// ---------------------------------------------------------------------------
builder.Services
    .AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection(CorsSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ---------------------------------------------------------------------------
// Servicios
// ---------------------------------------------------------------------------
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddProblemDetails(options =>
{
    // Se anade la correlacion y nada mas: ningun detalle interno sale de aqui.
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["correlationId"] =
            context.HttpContext.GetCorrelationId();
        context.ProblemDetails.Instance = null;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live", "ready"]);

builder.Services.AddCors(options =>
{
    var settings = builder.Configuration
        .GetSection(CorsSettings.SectionName)
        .Get<CorsSettings>() ?? new CorsSettings();

    options.AddPolicy(CorsSettings.PolicyName, policy => policy
        .WithOrigins([.. settings.AllowedOrigins])
        .WithMethods("GET", "POST")
        .WithHeaders("Content-Type", CorrelationIdMiddleware.HeaderName)
        .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)
        .SetPreflightMaxAge(TimeSpan.FromHours(1)));
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline. El orden importa: la correlacion se establece antes que nada para
// que cualquier fallo posterior salga ya etiquetado.
// ---------------------------------------------------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(CorsSettings.PolicyName);

// El documento OpenAPI es el contrato con collective-web. En desarrollo se
// sirve para poder mirarlo; en CI se genera como artefacto.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

await app.RunAsync().ConfigureAwait(false);

/// <summary>Punto de entrada expuesto para <c>WebApplicationFactory</c>.</summary>
[SuppressMessage("Design", "CA1050:Declare types in namespaces",
    Justification = "Program es el punto de entrada; los tests de integracion lo referencian.")]
public partial class Program
{
    protected Program()
    {
    }
}
