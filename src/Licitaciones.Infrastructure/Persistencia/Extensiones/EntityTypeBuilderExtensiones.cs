using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistencia.Extensiones;

public static class EntityTypeBuilderExtensiones
{
    /// <summary>
    /// Usa la columna de sistema <c>xmin</c> de PostgreSQL como token de
    /// concurrencia optimista (spec §11), mediante una propiedad sombra que
    /// no requiere modelarse en el dominio.
    /// </summary>
    public static void UsarXminComoTokenConcurrencia<TEntidad>(this EntityTypeBuilder<TEntidad> builder)
        where TEntidad : class
    {
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
