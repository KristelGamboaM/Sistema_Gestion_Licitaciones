using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Servicios;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Servicios;

public class ResolutorNivelAprobacionTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private static readonly IReadOnlyList<NivelAprobacion> Niveles =
    [
        NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj),
        NivelAprobacion.Crear(1_000_000m, 9_999_999.99m, "Gerencia", Reloj),
        NivelAprobacion.Crear(10_000_000m, null, "Junta Directiva", Reloj),
    ];

    [Theory]
    [InlineData(500_000, "Encargado de área")]
    [InlineData(1_000_000, "Gerencia")]
    [InlineData(9_999_999.99, "Gerencia")]
    [InlineData(10_000_000, "Junta Directiva")]
    [InlineData(50_000_000, "Junta Directiva")]
    public void Resolver_devuelveElNivelQueContieneElMonto(decimal monto, string aprobadorEsperado)
    {
        var nivel = ResolutorNivelAprobacion.Resolver(Niveles, monto);

        Assert.NotNull(nivel);
        Assert.Equal(aprobadorEsperado, nivel!.Aprobador);
    }

    [Fact]
    public void Resolver_sinNingunRangoQueCubraElMonto_devuelveNull()
    {
        var nivel = ResolutorNivelAprobacion.Resolver(Niveles, 0m);

        Assert.Null(nivel);
    }
}
