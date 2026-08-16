using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Servicios;

public sealed record ResultadoMejorOferta(Oferta? Mejor, decimal PorcentajeAhorro, ClasificacionOferta Clasificacion)
{
    public static ResultadoMejorOferta SinOfertas() =>
        new(Mejor: null, PorcentajeAhorro: 0m, ClasificacionOferta.SinOfertasValidas);
}
