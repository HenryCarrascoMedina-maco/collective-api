namespace Collective.Api.Common.Configuration;

/// <summary>Limites de tamano de peticion. PLAN_BACKEND.md 5.3.</summary>
public static class RequestLimits
{
    /// <summary>64 KB. Un mensaje legitimo del formulario no ocupa mas.</summary>
    public const long MaxBodyBytes = 64 * 1024;
}
