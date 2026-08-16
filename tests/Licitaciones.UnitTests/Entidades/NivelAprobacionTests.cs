using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Entidades;

public class NivelAprobacionTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    [Fact]
    public void Crear_conRangoCerradoValido_asignaCampos()
    {
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);

        Assert.False(nivel.EsRangoAbierto);
        Assert.Equal("Encargado de área", nivel.Aprobador);
    }

    [Fact]
    public void Crear_sinMontoMaximo_esRangoAbierto()
    {
        var nivel = NivelAprobacion.Crear(10_000_000m, null, "Junta Directiva", Reloj);

        Assert.True(nivel.EsRangoAbierto);
    }

    [Fact]
    public void Crear_conMontoMinimoNoPositivo_lanzaReglaNegocio()
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(
            () => NivelAprobacion.Crear(0m, 100m, "Aprobador", Reloj));

        Assert.Equal("nivel.monto_minimo.invalido", excepcion.Codigo);
    }

    [Fact]
    public void Crear_conMaximoMenorOIgualAlMinimo_lanzaReglaNegocio()
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(
            () => NivelAprobacion.Crear(1000m, 1000m, "Aprobador", Reloj));

        Assert.Equal("nivel.rango.invalido", excepcion.Codigo);
    }

    [Theory]
    [InlineData(500_000, true)]
    [InlineData(1_000_000, false)]
    [InlineData(0.01, true)]
    public void Contiene_evaluaLimitesDelRango(decimal monto, bool esperado)
    {
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);

        Assert.Equal(esperado, nivel.Contiene(monto));
    }

    [Fact]
    public void SeTraslapaCon_rangosDisjuntos_esFalse()
    {
        var nivelUno = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);
        var nivelDos = NivelAprobacion.Crear(1_000_000m, 9_999_999.99m, "Gerencia", Reloj);

        Assert.False(nivelUno.SeTraslapaCon(nivelDos));
    }

    [Fact]
    public void SeTraslapaCon_rangosQueSeCruzan_esTrue()
    {
        var nivelUno = NivelAprobacion.Crear(0.01m, 1_000_000m, "Encargado de área", Reloj);
        var nivelDos = NivelAprobacion.Crear(500_000m, 2_000_000m, "Gerencia", Reloj);

        Assert.True(nivelUno.SeTraslapaCon(nivelDos));
    }

    [Fact]
    public void SeTraslapaCon_dosRangosAbiertos_esTrue()
    {
        var nivelUno = NivelAprobacion.Crear(5_000_000m, null, "Gerencia", Reloj);
        var nivelDos = NivelAprobacion.Crear(10_000_000m, null, "Junta Directiva", Reloj);

        Assert.True(nivelUno.SeTraslapaCon(nivelDos));
    }
}
