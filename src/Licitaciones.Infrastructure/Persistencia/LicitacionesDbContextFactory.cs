using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Licitaciones.Infrastructure.Persistencia;

/// <summary>
/// Fábrica de tiempo de diseño usada únicamente por las herramientas de EF
/// Core (<c>dotnet ef migrations</c>) para construir el DbContext sin
/// necesitar levantar la aplicación Web/Api completa. No se usa en runtime.
/// </summary>
public sealed class LicitacionesDbContextFactory : IDesignTimeDbContextFactory<LicitacionesDbContext>
{
    public LicitacionesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LicitacionesDb")
            ?? "Host=localhost;Port=5432;Database=licitaciones;Username=licitaciones;Password=licitaciones";

        var optionsBuilder = new DbContextOptionsBuilder<LicitacionesDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LicitacionesDbContext(optionsBuilder.Options);
    }
}
