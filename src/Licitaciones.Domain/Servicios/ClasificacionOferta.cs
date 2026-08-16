namespace Licitaciones.Domain.Servicios;

public enum ClasificacionOferta
{
    SinOfertasValidas,
    OfertaConveniente,
    OfertaAceptable,
    OfertaValidaSinAhorro,
}

public static class ClasificacionOfertaExtensiones
{
    public static string AMensaje(this ClasificacionOferta clasificacion) => clasificacion switch
    {
        ClasificacionOferta.SinOfertasValidas => "Sin ofertas válidas",
        ClasificacionOferta.OfertaConveniente => "Oferta conveniente",
        ClasificacionOferta.OfertaAceptable => "Oferta aceptable",
        ClasificacionOferta.OfertaValidaSinAhorro => "Oferta válida sin ahorro",
        _ => throw new ArgumentOutOfRangeException(nameof(clasificacion)),
    };
}
