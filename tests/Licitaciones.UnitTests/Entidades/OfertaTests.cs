using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Entidades;

public class OfertaTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    [Fact]
    public void Registrar_conMontoPositivo_asignaFechaRegistro()
    {
        var oferta = Oferta.Registrar(Guid.NewGuid(), Guid.NewGuid(), 500_000m, Reloj);

        Assert.Equal(500_000m, oferta.MontoOfertadoCRC);
        Assert.Equal(Reloj.UtcAhora, oferta.FechaRegistro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Registrar_conMontoNoPositivo_lanzaReglaNegocio(decimal monto)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(
            () => Oferta.Registrar(Guid.NewGuid(), Guid.NewGuid(), monto, Reloj));

        Assert.Equal("oferta.monto.invalido", excepcion.Codigo);
    }

    [Fact]
    public void ActualizarMonto_conMontoPositivo_actualizaYRegistraFecha()
    {
        var oferta = Oferta.Registrar(Guid.NewGuid(), Guid.NewGuid(), 500_000m, Reloj);
        var reloj = RelojFalso.EnUtc(2026, 1, 11);

        oferta.ActualizarMonto(600_000m, reloj);

        Assert.Equal(600_000m, oferta.MontoOfertadoCRC);
        Assert.Equal(reloj.UtcAhora, oferta.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ActualizarMonto_conMontoNoPositivo_lanzaReglaNegocio(decimal monto)
    {
        var oferta = Oferta.Registrar(Guid.NewGuid(), Guid.NewGuid(), 500_000m, Reloj);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => oferta.ActualizarMonto(monto, Reloj));

        Assert.Equal("oferta.monto.invalido", excepcion.Codigo);
    }
}
