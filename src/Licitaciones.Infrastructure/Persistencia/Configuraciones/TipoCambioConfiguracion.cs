using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Extensiones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Configuraciones;

public sealed class TipoCambioConfiguracion : IEntityTypeConfiguration<TipoCambio>
{
    public void Configure(EntityTypeBuilder<TipoCambio> builder)
    {
        builder.ToTable("tipos_cambio", tb =>
            tb.HasCheckConstraint("CK_tipos_cambio_tasa_positiva", "\"CRCporUSD\" > 0"));

        builder.HasKey(t => t.Id);

        // Precisión mayor que el resto de montos: es una tasa, no un monto en colones.
        builder.Property(t => t.CRCporUSD)
            .HasColumnType("numeric(18,6)")
            .IsRequired();

        builder.Property(t => t.FechaVigencia).IsRequired();
        builder.Property(t => t.Activo).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // Solo puede existir un tipo de cambio activo a la vez (defensa en profundidad
        // además de la regla de aplicación que desactiva los demás al activar uno).
        builder.HasIndex(t => t.Activo)
            .IsUnique()
            .HasFilter("\"Activo\" = true");

        builder.UsarXminComoTokenConcurrencia();
        builder.HasData(DatosSemilla.TiposCambio());
    }
}
