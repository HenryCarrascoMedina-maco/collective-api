namespace Collective.Api.Modules.Contact;

/// <summary>
/// Traduce entre el valor publicado en el contrato (kebab-case, estable) y el
/// enum interno. Se mantienen separados a proposito: renombrar el enum no debe
/// romper a ningun cliente (PLAN_GENERAL.md 7.1).
/// </summary>
public static class ContactReasonMap
{
    public const string Trial = "trial";
    public const string CustomDev = "custom-dev";
    public const string Consulting = "consulting";
    public const string Partnership = "partnership";
    public const string Other = "other";

    /// <summary>Valores aceptados. Lista blanca del contrato (regla S2).</summary>
    public static readonly string[] Allowed =
        [Trial, CustomDev, Consulting, Partnership, Other];

    /// <summary>Convierte un valor ya validado. Lanza si no esta en la lista.</summary>
    public static ContactReason Parse(string wireValue) => wireValue switch
    {
        Trial => ContactReason.Trial,
        CustomDev => ContactReason.CustomDev,
        Consulting => ContactReason.Consulting,
        Partnership => ContactReason.Partnership,
        Other => ContactReason.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(wireValue), wireValue, "Unknown reason."),
    };
}
