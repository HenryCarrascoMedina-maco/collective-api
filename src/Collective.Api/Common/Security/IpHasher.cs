using System.Security.Cryptography;
using System.Text;

namespace Collective.Api.Common.Security;

/// <summary>
/// Hash con sal de la direccion IP. Regla S5: la IP nunca se guarda en claro.
/// La sal vive en configuracion; sin ella no se puede revertir el hash a partir
/// del reducido espacio de direcciones IPv4.
/// </summary>
public sealed class IpHasher(IConfiguration configuration)
{
    private readonly byte[] _salt = Encoding.UTF8.GetBytes(
        configuration["Security:IpHashSalt"] ?? "development-only-salt");

    public string? Hash(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var input = new byte[_salt.Length + Encoding.UTF8.GetByteCount(ipAddress)];
        _salt.CopyTo(input, 0);
        Encoding.UTF8.GetBytes(ipAddress, input.AsSpan(_salt.Length));

        return Convert.ToHexStringLower(SHA256.HashData(input));
    }
}
