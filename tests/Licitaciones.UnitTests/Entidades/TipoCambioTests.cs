using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Entidades;

public class TipoCambioTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    [Fact]
    public void Crear_conTasaValida_quedaInactivoPorDefecto()
    {
        var tipoCambio = TipoCambio.Crear(520.50m, Reloj.UtcAhora, Reloj);

        Assert.Equal(520.50m, tipoCambio.CRCporUSD);
        Assert.False(tipoCambio.Activo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Crear_conTasaNoPositiva_lanzaReglaNegocio(decimal tasa)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(
            () => TipoCambio.Crear(tasa, Reloj.UtcAhora, Reloj));

        Assert.Equal("tipo_cambio.tasa.invalida", excepcion.Codigo);
    }

    [Fact]
    public void Activar_marcaActivoTrue()
    {
        var tipoCambio = TipoCambio.Crear(500m, Reloj.UtcAhora, Reloj);

        tipoCambio.Activar(Reloj);

        Assert.True(tipoCambio.Activo);
    }

    [Fact]
    public void ConvertirCrcAUsd_calculaDivisionRedondeadaADosDecimales()
    {
        var tipoCambio = TipoCambio.Crear(500m, Reloj.UtcAhora, Reloj);

        var montoUsd = tipoCambio.ConvertirCrcAUsd(1_000_000m);

        Assert.Equal(2000.00m, montoUsd);
    }

    [Fact]
    public void ConvertirCrcAUsd_noAlteraElMontoOriginalEnCrc()
    {
        var tipoCambio = TipoCambio.Crear(517.35m, Reloj.UtcAhora, Reloj);

        tipoCambio.ConvertirCrcAUsd(123_456.78m);

        Assert.Equal(517.35m, tipoCambio.CRCporUSD);
    }
}
