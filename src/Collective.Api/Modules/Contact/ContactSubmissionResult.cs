namespace Collective.Api.Modules.Contact;

/// <summary>Resultado del envio. El cliente solo necesita saber que se acepto.</summary>
public sealed record ContactSubmissionResult(bool Ok)
{
    public static ContactSubmissionResult Accepted { get; } = new(true);
}
