using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Domain.Entidades;

public sealed class Oferta : EntidadBase
{
    public Guid LicitacionId { get; private set; }
    public Guid ProveedorId { get; private set; }
    public decimal MontoOfertadoCRC { get; private set; }
    public DateTimeOffset FechaRegistro { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Oferta()
    {
        // Requerido por EF Core.
    }

    /// <summary>
    /// Registra una oferta. Las reglas que dependen de otras entidades
    /// (duplicidad por proveedor, licitación publicada y no vencida,
    /// monto no mayor al presupuesto) se validan en la capa de aplicación
    /// antes de invocar este método, que solo protege el invariante propio
    /// del monto.
    /// </summary>
    public static Oferta Registrar(Guid licitacionId, Guid proveedorId, decimal montoOfertadoCRC, IReloj reloj)
    {
        if (montoOfertadoCRC <= 0)
            throw new ReglaNegocioException("oferta.monto.invalido", "El monto ofertado debe ser mayor que cero.");

        var ahora = reloj.UtcAhora;
        return new Oferta
        {
            LicitacionId = licitacionId,
            ProveedorId = proveedorId,
            MontoOfertadoCRC = montoOfertadoCRC,
            FechaRegistro = ahora,
            UpdatedAt = ahora,
        };
    }

    /// <summary>
    /// Cambia el monto de una oferta ya registrada. Igual que en
    /// <see cref="Registrar"/>, las reglas que dependen de la licitación
    /// (no vencida, no superar presupuesto) se validan en la aplicación.
    /// </summary>
    public void ActualizarMonto(decimal montoOfertadoCRC, IReloj reloj)
    {
        if (montoOfertadoCRC <= 0)
            throw new ReglaNegocioException("oferta.monto.invalido", "El monto ofertado debe ser mayor que cero.");

        MontoOfertadoCRC = montoOfertadoCRC;
        UpdatedAt = reloj.UtcAhora;
    }
}
