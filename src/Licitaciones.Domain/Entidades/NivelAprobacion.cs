using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Domain.Entidades;

public sealed class NivelAprobacion : EntidadBase
{
    public decimal MontoMinimoCRC { get; private set; }
    public decimal? MontoMaximoCRC { get; private set; }
    public string Aprobador { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool EsRangoAbierto => MontoMaximoCRC is null;

    private NivelAprobacion()
    {
        // Requerido por EF Core.
    }

    public static NivelAprobacion Crear(decimal montoMinimoCRC, decimal? montoMaximoCRC, string aprobador, IReloj reloj)
    {
        var nivel = new NivelAprobacion { CreatedAt = reloj.UtcAhora };
        nivel.Actualizar(montoMinimoCRC, montoMaximoCRC, aprobador, reloj);
        return nivel;
    }

    public void Actualizar(decimal montoMinimoCRC, decimal? montoMaximoCRC, string aprobador, IReloj reloj)
    {
        if (montoMinimoCRC <= 0)
            throw new ReglaNegocioException("nivel.monto_minimo.invalido", "El monto mínimo debe ser mayor que cero.");

        if (montoMaximoCRC is not null && montoMaximoCRC <= montoMinimoCRC)
        {
            throw new ReglaNegocioException(
                "nivel.rango.invalido",
                "El monto máximo debe ser mayor que el monto mínimo.");
        }

        if (string.IsNullOrWhiteSpace(aprobador))
            throw new ReglaNegocioException("nivel.aprobador.requerido", "El aprobador es obligatorio.");

        MontoMinimoCRC = montoMinimoCRC;
        MontoMaximoCRC = montoMaximoCRC;
        Aprobador = aprobador.Trim();
        UpdatedAt = reloj.UtcAhora;
    }

    /// <summary>Indica si este rango se traslapa con otro (usado al validar la colección completa).</summary>
    public bool SeTraslapaCon(NivelAprobacion otro)
    {
        var esteMaximo = MontoMaximoCRC ?? decimal.MaxValue;
        var otroMaximo = otro.MontoMaximoCRC ?? decimal.MaxValue;
        return MontoMinimoCRC <= otroMaximo && otro.MontoMinimoCRC <= esteMaximo;
    }

    public bool Contiene(decimal monto) =>
        monto >= MontoMinimoCRC && (MontoMaximoCRC is null || monto <= MontoMaximoCRC);
}
