using System.ComponentModel.DataAnnotations;

namespace Collective.Api.Common.Configuration;

/// <summary>
/// Origenes permitidos, explicitos por entorno. Nunca AllowAnyOrigin (regla S7).
/// </summary>
public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "collective-web";

    [Required]
    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];
}
