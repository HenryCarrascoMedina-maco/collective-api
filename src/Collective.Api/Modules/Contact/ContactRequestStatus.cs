namespace Collective.Api.Modules.Contact;

/// <summary>Estado del lead. En V1 solo se crea como <see cref="New"/>.</summary>
public enum ContactRequestStatus
{
    New,
    Read,
    Answered,
    Archived,
}
