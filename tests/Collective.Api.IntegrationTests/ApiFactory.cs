using Collective.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Collective.Api.IntegrationTests;

/// <summary>
/// Levanta la API en memoria con SQLite como almacen.
///
/// El plan pide Testcontainers con Postgres real; mientras Docker esta aparcado,
/// SQLite permite ejercitar el endpoint de extremo a extremo contra EF Core de
/// verdad. Se cambiara a Testcontainers cuando Docker vuelva al alcance.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly int _permitLimit;

    /// <summary>Fabrica compartida por una clase de tests (xUnit la crea sin argumentos).</summary>
    public ApiFactory() : this(permitLimit: 1_000)
    {
    }

    private ApiFactory(int permitLimit) => _permitLimit = permitLimit;

    /// <summary>Fabrica con un limite propio, para el test que comprueba el limitador.
    /// xUnit exige un unico constructor publico en un fixture, de ahi la fabrica.</summary>
    public static ApiFactory WithPermitLimit(int permitLimit) => new(permitLimit);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseEnvironment("Production");

        // Por defecto el limitador se abre: si no, unos tests agotarian la cuota
        // de los otros. El test que lo comprueba usa su propio limite bajo.
        builder.UseSetting("RateLimiting:PublicWrite:PermitLimit", _permitLimit.ToString(
            System.Globalization.CultureInfo.InvariantCulture));

        builder.ConfigureServices(services =>
        {
            RemoveNpgsql(services);

            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options
                .UseSqlite(_connection)
                .UseSnakeCaseNamingConvention());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return host;
    }

    /// <summary>Da acceso al almacen para comprobar lo que quedo guardado.</summary>
    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await action(db).ConfigureAwait(false);
    }

    /// <summary>
    /// EF solo admite un proveedor por contenedor, asi que hay que retirar el de
    /// Postgres que registra la aplicacion antes de poner el de SQLite.
    /// </summary>
    private static void RemoveNpgsql(IServiceCollection services)
    {
        var doomed = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                || descriptor.ServiceType == typeof(DbContextOptions)
                || descriptor.ServiceType == typeof(AppDbContext)
                || IsOptionsConfiguration(descriptor.ServiceType)
                || IsNpgsql(descriptor.ServiceType)
                || IsNpgsql(descriptor.ImplementationType))
            .ToList();

        foreach (var descriptor in doomed)
        {
            services.Remove(descriptor);
        }

        // Desde .NET 9 AddDbContext registra la configuracion de opciones y se
        // aplican TODAS: si no se retira la de Npgsql, se suma a la de SQLite y
        // EF ve dos proveedores.
        static bool IsOptionsConfiguration(Type type) =>
            type.IsGenericType
            && type.GetGenericTypeDefinition().Name.StartsWith(
                "IDbContextOptionsConfiguration", StringComparison.Ordinal);

        static bool IsNpgsql(Type? type) =>
            type?.Namespace?.StartsWith("Npgsql", StringComparison.Ordinal) == true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
