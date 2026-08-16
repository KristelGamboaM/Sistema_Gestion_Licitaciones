using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Domain.Servicios;

namespace Licitaciones.Domain.Entidades;

public sealed class Proveedor : EntidadBase
{
    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool EstaEliminado => DeletedAt is not null;

    private Proveedor()
    {
        // Requerido por EF Core.
    }

    public static Proveedor Crear(string nombre, IReloj reloj)
    {
        var proveedor = new Proveedor { CreatedAt = reloj.UtcAhora };
        proveedor.EstablecerNombre(nombre);
        proveedor.UpdatedAt = reloj.UtcAhora;
        return proveedor;
    }

    public void Actualizar(string nombre, IReloj reloj)
    {
        if (EstaEliminado)
            throw new ReglaNegocioException("proveedor.eliminado", "No se puede editar un proveedor eliminado.");

        EstablecerNombre(nombre);
        UpdatedAt = reloj.UtcAhora;
    }

    public void Eliminar(IReloj reloj)
    {
        if (EstaEliminado)
            return;

        DeletedAt = reloj.UtcAhora;
        UpdatedAt = reloj.UtcAhora;
    }

    private void EstablecerNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("proveedor.nombre.requerido", "El nombre del proveedor es obligatorio.");

        var nombreRecortado = nombre.Trim();
        if (!NormalizacionTexto.TieneCaracteresPermitidosProveedor(nombreRecortado))
        {
            throw new ReglaNegocioException(
                "proveedor.nombre.caracteres_invalidos",
                "El nombre del proveedor solo admite letras, números, espacios, punto, coma y paréntesis.");
        }

        Nombre = nombreRecortado;
        NombreNormalizado = NormalizacionTexto.NormalizarNombreProveedor(nombreRecortado);
    }
}
