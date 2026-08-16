using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Domain.Entidades;

public sealed class TipoCambio : EntidadBase
{
    public decimal CRCporUSD { get; private set; }
    public DateTimeOffset FechaVigencia { get; private set; }
    public bool Activo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private TipoCambio()
    {
        // Requerido por EF Core.
    }

    public static TipoCambio Crear(decimal crcPorUsd, DateTimeOffset fechaVigencia, IReloj reloj)
    {
        var tipoCambio = new TipoCambio { CreatedAt = reloj.UtcAhora };
        tipoCambio.Actualizar(crcPorUsd, fechaVigencia, reloj);
        return tipoCambio;
    }

    public void Actualizar(decimal crcPorUsd, DateTimeOffset fechaVigencia, IReloj reloj)
    {
        if (crcPorUsd <= 0)
            throw new ReglaNegocioException("tipo_cambio.tasa.invalida", "La tasa de cambio debe ser mayor que cero.");

        CRCporUSD = crcPorUsd;
        FechaVigencia = fechaVigencia;
        UpdatedAt = reloj.UtcAhora;
    }

    public void Activar(IReloj reloj)
    {
        Activo = true;
        UpdatedAt = reloj.UtcAhora;
    }

    public void Desactivar(IReloj reloj)
    {
        Activo = false;
        UpdatedAt = reloj.UtcAhora;
    }

    /// <summary>USD es siempre una representación calculada; nunca se persiste.</summary>
    public decimal ConvertirCrcAUsd(decimal montoCRC) =>
        Math.Round(montoCRC / CRCporUSD, 2, MidpointRounding.AwayFromZero);
}
