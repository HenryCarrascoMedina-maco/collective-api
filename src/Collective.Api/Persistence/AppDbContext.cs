using Microsoft.EntityFrameworkCore;

namespace Collective.Api.Persistence;

/// <summary>
/// Contexto unico. Cada modulo aporta su configuracion; el contexto no conoce
/// las reglas de ninguno (regla M4).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
