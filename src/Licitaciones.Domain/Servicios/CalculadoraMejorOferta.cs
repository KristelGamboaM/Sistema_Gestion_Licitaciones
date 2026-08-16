using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Servicios;

/// <summary>
/// Determina la mejor oferta de una licitación y su clasificación de
/// ahorro (spec §8.6). Toda oferta persistida ya es válida por construcción
/// (las reglas de rechazo se aplican antes de registrarla), por lo que este
/// cálculo opera sobre la colección completa sin filtrar de nuevo.
/// </summary>
public static class CalculadoraMejorOferta
{
    public static ResultadoMejorOferta Calcular(decimal presupuestoEstimadoCRC, IEnumerable<Oferta> ofertas)
    {
        var mejor = ofertas
            .OrderBy(o => o.MontoOfertadoCRC)
            .ThenBy(o => o.FechaRegistro)
            .FirstOrDefault();

        if (mejor is null)
            return ResultadoMejorOferta.SinOfertas();

        var porcentajeAhorro = presupuestoEstimadoCRC == 0
            ? 0m
            : (presupuestoEstimadoCRC - mejor.MontoOfertadoCRC) / presupuestoEstimadoCRC * 100m;

        var clasificacion = mejor.MontoOfertadoCRC == presupuestoEstimadoCRC
            ? ClasificacionOferta.OfertaValidaSinAhorro
            : porcentajeAhorro >= 10m
                ? ClasificacionOferta.OfertaConveniente
                : ClasificacionOferta.OfertaAceptable;

        return new ResultadoMejorOferta(mejor, porcentajeAhorro, clasificacion);
    }
}
