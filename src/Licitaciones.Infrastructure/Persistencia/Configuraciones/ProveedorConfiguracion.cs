using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia.Extensiones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Configuraciones;

public sealed class ProveedorConfiguracion : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.NombreNormalizado)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(p => p.NombreNormalizado)
            .IsUnique();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.UsarXminComoTokenConcurrencia();
    }
}
