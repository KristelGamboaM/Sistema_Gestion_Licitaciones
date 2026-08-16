using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;
using Moq;

namespace Licitaciones.UnitTests.Aplicacion;

public class TipoCambioAppServiceTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private readonly Mock<ITipoCambioRepository> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TipoCambioAppService _servicio;

    public TipoCambioAppServiceTests()
    {
        _servicio = new TipoCambioAppService(_repositorio.Object, _unitOfWork.Object, Reloj);
    }

    [Fact]
    public async Task CrearAsync_agregaInactivoPorDefecto()
    {
        var resultado = await _servicio.CrearAsync(new GuardarTipoCambioRequest(520m, Reloj.UtcAhora));

        Assert.False(resultado.Activo);
        _repositorio.Verify(r => r.Agregar(It.IsAny<TipoCambio>()), Times.Once);
    }

    [Fact]
    public async Task ActivarAsync_desactivaElAnteriorYActivaElNuevo()
    {
        var anterior = TipoCambio.Crear(500m, Reloj.UtcAhora, Reloj);
        anterior.Activar(Reloj);
        var nuevo = TipoCambio.Crear(530m, Reloj.UtcAhora, Reloj);

        _repositorio.Setup(r => r.ObtenerActivoAsync(default)).ReturnsAsync(anterior);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(nuevo.Id, default)).ReturnsAsync(nuevo);

        var resultado = await _servicio.ActivarAsync(nuevo.Id);

        Assert.True(resultado.Activo);
        Assert.False(anterior.Activo);
    }

    [Fact]
    public async Task ConvertirCrcAUsdAsync_sinTipoDeCambioActivo_lanzaReglaNegocio()
    {
        _repositorio.Setup(r => r.ObtenerActivoAsync(default)).ReturnsAsync((TipoCambio?)null);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.ConvertirCrcAUsdAsync(1000m));

        Assert.Equal("tipo_cambio.sin_activo", excepcion.Codigo);
    }

    [Fact]
    public async Task ConvertirCrcAUsdAsync_conTipoDeCambioActivo_calculaMontoUsd()
    {
        var activo = TipoCambio.Crear(500m, Reloj.UtcAhora, Reloj);
        activo.Activar(Reloj);
        _repositorio.Setup(r => r.ObtenerActivoAsync(default)).ReturnsAsync(activo);

        var resultado = await _servicio.ConvertirCrcAUsdAsync(1_000_000m);

        Assert.Equal(2000.00m, resultado.MontoUSD);
        Assert.Equal(500m, resultado.CRCporUSD);
    }

    [Fact]
    public async Task ObtenerAsync_conIdInexistente_lanzaEntidadNoEncontrada()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TipoCambio?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.ObtenerAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarAsync_devuelveTodosLosTiposDeCambioMapeados()
    {
        var tipoCambio = TipoCambio.Crear(520m, Reloj.UtcAhora, Reloj);
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([tipoCambio]);

        var resultado = await _servicio.ListarAsync();

        Assert.Single(resultado);
        Assert.Equal(520m, resultado[0].CRCporUSD);
    }

    [Fact]
    public async Task ObtenerActivoAsync_sinActivo_devuelveNull()
    {
        _repositorio.Setup(r => r.ObtenerActivoAsync(default)).ReturnsAsync((TipoCambio?)null);

        var resultado = await _servicio.ObtenerActivoAsync();

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ActualizarAsync_conTasaValida_actualizaYGuarda()
    {
        var tipoCambio = TipoCambio.Crear(500m, Reloj.UtcAhora, Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(tipoCambio.Id, default)).ReturnsAsync(tipoCambio);

        var resultado = await _servicio.ActualizarAsync(
            tipoCambio.Id, new GuardarTipoCambioRequest(540m, Reloj.UtcAhora));

        Assert.Equal(540m, resultado.CRCporUSD);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }
}
