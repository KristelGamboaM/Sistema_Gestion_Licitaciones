using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Servicios;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Servicios;

public class CalculadoraMejorOfertaTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);
    private static readonly Guid LicitacionId = Guid.NewGuid();

    private static Oferta OfertaDe(decimal monto, RelojFalso reloj)
    {
        var oferta = Oferta.Registrar(LicitacionId, Guid.NewGuid(), monto, reloj);
        reloj.AvanzarA(reloj.UtcAhora.AddMinutes(1));
        return oferta;
    }

    [Fact]
    public void Calcular_sinOfertas_devuelveSinOfertasValidas()
    {
        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, []);

        Assert.Null(resultado.Mejor);
        Assert.Equal(ClasificacionOferta.SinOfertasValidas, resultado.Clasificacion);
    }

    [Fact]
    public void Calcular_ofertaIgualAlPresupuesto_esValidaSinAhorro()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var oferta = OfertaDe(1_000_000m, reloj);

        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, [oferta]);

        Assert.Equal(0m, resultado.PorcentajeAhorro);
        Assert.Equal(ClasificacionOferta.OfertaValidaSinAhorro, resultado.Clasificacion);
    }

    [Fact]
    public void Calcular_ahorroDe10PorCientoOMas_esConveniente()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var oferta = OfertaDe(900_000m, reloj); // 10% de ahorro exacto

        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, [oferta]);

        Assert.Equal(10m, resultado.PorcentajeAhorro);
        Assert.Equal(ClasificacionOferta.OfertaConveniente, resultado.Clasificacion);
    }

    [Fact]
    public void Calcular_ahorroEntreCeroYDiezPorCiento_esAceptable()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var oferta = OfertaDe(950_000m, reloj); // 5% de ahorro

        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, [oferta]);

        Assert.Equal(5m, resultado.PorcentajeAhorro);
        Assert.Equal(ClasificacionOferta.OfertaAceptable, resultado.Clasificacion);
    }

    [Fact]
    public void Calcular_conVariasOfertas_eligeElMenorMonto()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var ofertaAlta = OfertaDe(900_000m, reloj);
        var ofertaBaja = OfertaDe(700_000m, reloj);
        var ofertaMedia = OfertaDe(800_000m, reloj);

        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, [ofertaAlta, ofertaBaja, ofertaMedia]);

        Assert.Equal(ofertaBaja, resultado.Mejor);
    }

    [Fact]
    public void Calcular_conEmpate_eligeLaOfertaRegistradaPrimero()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var primeraOferta = OfertaDe(800_000m, reloj);
        var segundaOferta = OfertaDe(800_000m, reloj);

        var resultado = CalculadoraMejorOferta.Calcular(1_000_000m, [segundaOferta, primeraOferta]);

        Assert.Equal(primeraOferta, resultado.Mejor);
    }
}
