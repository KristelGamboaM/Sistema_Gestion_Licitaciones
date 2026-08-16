using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Extensiones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Configuraciones;

public sealed class OfertaConfiguracion : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> builder)
    {
        builder.ToTable("ofertas", t =>
            t.HasCheckConstraint("CK_ofertas_monto_positivo", "\"MontoOfertadoCRC\" > 0"));

        builder.HasKey(o => o.Id);

        builder.Property(o => o.MontoOfertadoCRC)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(o => o.FechaRegistro).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        // Un proveedor no puede registrar más de una oferta para la misma licitación.
        builder.HasIndex(o => new { o.LicitacionId, o.ProveedorId }).IsUnique();

        builder.HasOne<Licitacion>()
            .WithMany()
            .HasForeignKey(o => o.LicitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Proveedor>()
            .WithMany()
            .HasForeignKey(o => o.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.UsarXminComoTokenConcurrencia();
    }
}
