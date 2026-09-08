using System.ComponentModel.DataAnnotations;

namespace Collective.Api.Modules.Contact;

/// <summary>
/// Cuerpo del formulario. Validacion por lista blanca: longitudes y formato
/// explicitos (regla S2). Nunca es la entidad de dominio (regla A5).
/// </summary>
public sealed record SubmitContactRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [StringLength(160)]
    public string? Company { get; init; }

    [StringLength(40)]
    public string? Phone { get; init; }

    /// <summary>Uno de los valores de <see cref="ContactReasonMap.Allowed"/>.</summary>
    [Required]
    [AllowedValues(ContactReasonMap.Trial, ContactReasonMap.CustomDev,
        ContactReasonMap.Consulting, ContactReasonMap.Partnership, ContactReasonMap.Other)]
    public string Reason { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(4000, MinimumLength = 10)]
    public string Message { get; init; } = string.Empty;

    [StringLength(80)]
    public string? ProductSlug { get; init; }

    [StringLength(300)]
    public string? SourcePage { get; init; }

    /// <summary>
    /// Trampa para robots: un campo que una persona nunca ve ni rellena. Si
    /// llega con contenido, la peticion se descarta en silencio.
    /// </summary>
    [StringLength(200)]
    public string? Website { get; init; }
}
