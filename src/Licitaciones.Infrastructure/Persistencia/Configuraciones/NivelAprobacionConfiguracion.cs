using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Extensiones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Configuraciones;

public sealed class NivelAprobacionConfiguracion : IEntityTypeConfiguration<NivelAprobacion>
{
    public void Configure(EntityTypeBuilder<NivelAprobacion> builder)
    {
        builder.ToTable("niveles_aprobacion", t =>
        {
            t.HasCheckConstraint("CK_niveles_aprobacion_minimo_positivo", "\"MontoMinimoCRC\" > 0");
            t.HasCheckConstraint(
                "CK_niveles_aprobacion_rango_valido",
                "\"MontoMaximoCRC\" IS NULL OR \"MontoMaximoCRC\" > \"MontoMinimoCRC\"");
        });

        builder.HasKey(n => n.Id);

        builder.Property(n => n.MontoMinimoCRC)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(n => n.MontoMaximoCRC)
            .HasColumnType("numeric(18,2)");

        builder.Property(n => n.Aprobador)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        builder.UsarXminComoTokenConcurrencia();
        builder.HasData(DatosSemilla.NivelesAprobacion());
    }
}
