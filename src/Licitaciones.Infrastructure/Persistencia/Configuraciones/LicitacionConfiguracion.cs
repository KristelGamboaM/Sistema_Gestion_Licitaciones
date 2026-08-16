using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Extensiones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Configuraciones;

public sealed class LicitacionConfiguracion : IEntityTypeConfiguration<Licitacion>
{
    public void Configure(EntityTypeBuilder<Licitacion> builder)
    {
        builder.ToTable("licitaciones", t =>
            t.HasCheckConstraint("CK_licitaciones_presupuesto_positivo", "\"PresupuestoEstimadoCRC\" > 0"));

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Codigo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.CodigoNormalizado)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(l => l.CodigoNormalizado)
            .IsUnique();

        builder.Property(l => l.Titulo)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(l => l.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.PresupuestoEstimadoCRC)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(l => l.FechaCierre).IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt).IsRequired();

        builder.UsarXminComoTokenConcurrencia();
    }
}
