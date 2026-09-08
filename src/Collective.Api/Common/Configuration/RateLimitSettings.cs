using System.ComponentModel.DataAnnotations;

namespace Collective.Api.Common.Configuration;

/// <summary>
/// Limites del formulario publico. Van en configuracion, no en el codigo: el
/// valor razonable depende del entorno y no deberia exigir un despliegue.
/// </summary>
public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimiting:PublicWrite";

    [Range(1, 10_000)]
    public int PermitLimit { get; init; } = 5;

    [Range(1, 1_440)]
    public int WindowMinutes { get; init; } = 60;
}
