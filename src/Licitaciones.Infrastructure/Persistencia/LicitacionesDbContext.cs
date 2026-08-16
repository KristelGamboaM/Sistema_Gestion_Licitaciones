using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Conversiones;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistencia;

public sealed class LicitacionesDbContext(DbContextOptions<LicitacionesDbContext> options) : DbContext(options)
{
    public DbSet<Licitacion> Licitaciones => Set<Licitacion>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<NivelAprobacion> NivelesAprobacion => Set<NivelAprobacion>();
    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicitacionesDbContext).Assembly);
    }
}
