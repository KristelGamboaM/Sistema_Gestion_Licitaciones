using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Servicios;

/// <summary>
/// Resuelve el aprobador de un monto consultando la tabla parametrizable de
/// niveles de aprobación (spec §8.7) — nunca mediante una cadena if/else fija.
/// </summary>
public static class ResolutorNivelAprobacion
{
    public static NivelAprobacion? Resolver(IEnumerable<NivelAprobacion> niveles, decimal montoCRC) =>
        niveles.FirstOrDefault(nivel => nivel.Contiene(montoCRC));
}
