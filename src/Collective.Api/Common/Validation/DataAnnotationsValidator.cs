using System.ComponentModel.DataAnnotations;

namespace Collective.Api.Common.Validation;

/// <summary>
/// Validacion por lista blanca con DataAnnotations (regla S2). Sin dependencias:
/// la plataforma ya lo trae (regla P2).
/// </summary>
public static class DataAnnotationsValidator
{
    /// <summary>Devuelve los errores por campo, o vacio si la entrada es valida.</summary>
    public static Dictionary<string, string[]> Collect<T>(T instance)
        where T : notnull
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, member) => (Member: member, result.ErrorMessage))
            .GroupBy(entry => entry.Member, StringComparer.Ordinal)
            .ToDictionary(
                group => string.IsNullOrEmpty(group.Key)
                    ? group.Key
                    : char.ToLowerInvariant(group.Key[0]) + group.Key[1..],
                group => group.Select(entry => entry.ErrorMessage ?? "Invalid value").ToArray(),
                StringComparer.Ordinal);
    }
}
