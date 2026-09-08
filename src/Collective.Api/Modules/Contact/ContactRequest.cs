namespace Collective.Api.Modules.Contact;

/// <summary>
/// Solicitud de contacto. Unica entidad de la V1 (PLAN_GENERAL.md decision 6).
/// </summary>
public sealed class ContactRequest
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Name { get; init; }

    public required string Email { get; init; }

    public string? Company { get; init; }

    public string? Phone { get; init; }

    public required ContactReason Reason { get; init; }

    public required string Message { get; init; }

    public ContactRequestStatus Status { get; set; } = ContactRequestStatus.New;

    public string? ProductSlug { get; init; }

    public string? SourcePage { get; init; }

    public string? UserAgent { get; init; }

    /// <summary>Hash con sal. Nunca la direccion en claro (regla S5).</summary>
    public string? IpHash { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
