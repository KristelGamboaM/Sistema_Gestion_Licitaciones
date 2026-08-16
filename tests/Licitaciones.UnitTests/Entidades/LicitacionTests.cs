using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Entidades;

public class LicitacionTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private static Licitacion CrearLicitacionValida(RelojFalso reloj, decimal presupuesto = 1_000_000m) =>
        Licitacion.Crear("LIC-2026-001", "Compra de equipo de cómputo", presupuesto, reloj.UtcAhora.AddDays(10), reloj);

    [Fact]
    public void Crear_conDatosValidos_quedaEnBorrador()
    {
        var licitacion = CrearLicitacionValida(Reloj);

        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal("LIC-2026-001", licitacion.CodigoNormalizado);
    }

    [Theory]
    [InlineData(" lic-2026-001 ")]
    [InlineData("LIC-2026-001")]
    [InlineData("Lic-2026-001")]
    public void Crear_codigosEquivalentes_producenElMismoCodigoNormalizado(string codigo)
    {
        var licitacion = Licitacion.Crear(codigo, "Título", 100m, Reloj.UtcAhora.AddDays(1), Reloj);

        Assert.Equal("LIC-2026-001", licitacion.CodigoNormalizado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_conPresupuestoNoPositivo_lanzaReglaNegocio(decimal presupuesto)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(
            () => Licitacion.Crear("LIC-1", "Título", presupuesto, Reloj.UtcAhora.AddDays(1), Reloj));

        Assert.Equal("licitacion.presupuesto.invalido", excepcion.Codigo);
    }

    [Fact]
    public void Publicar_desdeBorradorConFechaFutura_pasaAPublicada()
    {
        var licitacion = CrearLicitacionValida(Reloj);

        licitacion.Publicar(Reloj);

        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void Publicar_conFechaCierreYaPasada_lanzaReglaNegocio()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var licitacion = Licitacion.Crear("LIC-1", "Título", 100m, reloj.UtcAhora.AddDays(5), reloj);
        reloj.AvanzarA(reloj.UtcAhora.AddDays(10));

        var excepcion = Assert.Throws<ReglaNegocioException>(() => licitacion.Publicar(reloj));

        Assert.Equal("licitacion.fecha_cierre.pasada", excepcion.Codigo);
    }

    [Fact]
    public void Publicar_dosVeces_lanzaTransicionInvalida()
    {
        var licitacion = CrearLicitacionValida(Reloj);
        licitacion.Publicar(Reloj);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => licitacion.Publicar(Reloj));

        Assert.Equal("licitacion.transicion.invalida", excepcion.Codigo);
    }

    [Theory]
    [MemberData(nameof(TransicionesDeCierrePermitidas))]
    public void Cerrar_desdeBorradorOPublicada_permiteElCierre(EstadoLicitacion estadoInicial)
    {
        var licitacion = CrearLicitacionValida(Reloj);
        if (estadoInicial == EstadoLicitacion.Publicada)
            licitacion.Publicar(Reloj);

        licitacion.Cerrar(Reloj);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
    }

    public static IEnumerable<object[]> TransicionesDeCierrePermitidas()
    {
        yield return [EstadoLicitacion.Borrador];
        yield return [EstadoLicitacion.Publicada];
    }

    [Fact]
    public void Cerrar_yaCerrada_esIdempotente()
    {
        var licitacion = CrearLicitacionValida(Reloj);
        licitacion.Cerrar(Reloj);

        licitacion.Cerrar(Reloj);

        Assert.Equal(EstadoLicitacion.Cerrada, licitacion.Estado);
    }

    [Fact]
    public void EsEfectivamenteCerrada_conFechaCierrePasada_esTrueAunqueEstadoDigaPublicada()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var licitacion = Licitacion.Crear("LIC-1", "Título", 100m, reloj.UtcAhora.AddDays(5), reloj);
        licitacion.Publicar(reloj);
        reloj.AvanzarA(reloj.UtcAhora.AddDays(10));

        Assert.True(licitacion.EsEfectivamenteCerrada(reloj));
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
    }

    [Fact]
    public void EsEfectivamenteCerrada_conFechaCierreFutura_esFalse()
    {
        var licitacion = CrearLicitacionValida(Reloj);
        licitacion.Publicar(Reloj);

        Assert.False(licitacion.EsEfectivamenteCerrada(Reloj));
    }

    [Fact]
    public void ActualizarDatosBorrador_presupuestoPordebajoDeOfertaExistente_lanzaReglaNegocio()
    {
        var licitacion = CrearLicitacionValida(Reloj, presupuesto: 1_000_000m);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => licitacion.ActualizarDatosBorrador(
            "LIC-2026-001", "Título", 500_000m, Reloj.UtcAhora.AddDays(10), montoMayorOfertaExistente: 800_000m, Reloj));

        Assert.Equal("licitacion.presupuesto.reduccion_invalida", excepcion.Codigo);
    }

    [Fact]
    public void ActualizarDatosBorrador_presupuestoIgualAOfertaExistente_esValido()
    {
        var licitacion = CrearLicitacionValida(Reloj, presupuesto: 1_000_000m);

        licitacion.ActualizarDatosBorrador(
            "LIC-2026-001", "Título", 800_000m, Reloj.UtcAhora.AddDays(10), montoMayorOfertaExistente: 800_000m, Reloj);

        Assert.Equal(800_000m, licitacion.PresupuestoEstimadoCRC);
    }

    [Fact]
    public void ActualizarDatosBorrador_sobreLicitacionPublicada_lanzaReglaNegocio()
    {
        var licitacion = CrearLicitacionValida(Reloj);
        licitacion.Publicar(Reloj);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => licitacion.ActualizarDatosBorrador(
            "LIC-2", "Otro título", 2_000_000m, Reloj.UtcAhora.AddDays(20), montoMayorOfertaExistente: null, Reloj));

        Assert.Equal("licitacion.edicion.no_permitida", excepcion.Codigo);
    }

    [Fact]
    public void Eliminar_marcaDeletedAtYEsIdempotente()
    {
        var licitacion = CrearLicitacionValida(Reloj);

        licitacion.Eliminar(Reloj);
        var primeraEliminacion = licitacion.DeletedAt;
        licitacion.Eliminar(Reloj);

        Assert.True(licitacion.EstaEliminada);
        Assert.Equal(primeraEliminacion, licitacion.DeletedAt);
    }
}
