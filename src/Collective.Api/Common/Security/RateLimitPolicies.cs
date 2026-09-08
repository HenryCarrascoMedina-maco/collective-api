namespace Collective.Api.Common.Security;

/// <summary>Nombres de las politicas de limitacion. Regla S6.</summary>
public static class RateLimitPolicies
{
    /// <summary>Envio del formulario publico: estricto, por IP.</summary>
    public const string PublicWrite = "public-write";
}
