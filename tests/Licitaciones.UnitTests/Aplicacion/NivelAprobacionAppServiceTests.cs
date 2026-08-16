using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;
using Moq;

namespace Licitaciones.UnitTests.Aplicacion;

public class NivelAprobacionAppServiceTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private readonly Mock<INivelAprobacionRepository> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly NivelAprobacionAppService _servicio;

    public NivelAprobacionAppServiceTests()
    {
        _servicio = new NivelAprobacionAppService(_repositorio.Object, _unitOfWork.Object, Reloj);
    }

    [Fact]
    public async Task CrearAsync_sinTraslape_agregaYGuarda()
    {
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([]);

        var resultado = await _servicio.CrearAsync(
            new GuardarNivelAprobacionRequest(0.01m, 999_999.99m, "Encargado de área"));

        Assert.Equal("Encargado de área", resultado.Aprobador);
        _repositorio.Verify(r => r.Agregar(It.IsAny<NivelAprobacion>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_conTraslape_lanzaConflicto()
    {
        var existente = NivelAprobacion.Crear(0.01m, 1_000_000m, "Encargado de área", Reloj);
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([existente]);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(
            new GuardarNivelAprobacionRequest(500_000m, 2_000_000m, "Gerencia")));

        Assert.Equal("nivel.rango.traslape", excepcion.Codigo);
        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
    }

    [Fact]
    public async Task CrearAsync_segundoRangoAbierto_lanzaConflicto()
    {
        var existente = NivelAprobacion.Crear(10_000_000m, null, "Junta Directiva", Reloj);
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([existente]);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(
            new GuardarNivelAprobacionRequest(20_000_000m, null, "Otro aprobador")));

        Assert.Equal("nivel.rango.traslape", excepcion.Codigo);
    }

    [Fact]
    public async Task ActualizarAsync_excluyeElPropioRegistroDelChequeoDeTraslape()
    {
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(nivel.Id, default)).ReturnsAsync(nivel);
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([nivel]);

        var resultado = await _servicio.ActualizarAsync(
            nivel.Id, new GuardarNivelAprobacionRequest(0.01m, 1_500_000m, "Encargado de área"));

        Assert.Equal(1_500_000m, resultado.MontoMaximoCRC);
    }

    [Fact]
    public async Task ObtenerAsync_conIdInexistente_lanzaEntidadNoEncontrada()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((NivelAprobacion?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.ObtenerAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarAsync_devuelveTodosLosNivelesMapeados()
    {
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);
        _repositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([nivel]);

        var resultado = await _servicio.ListarAsync();

        Assert.Single(resultado);
        Assert.Equal("Encargado de área", resultado[0].Aprobador);
    }

    [Fact]
    public async Task EliminarAsync_eliminaYGuarda()
    {
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(nivel.Id, default)).ReturnsAsync(nivel);

        await _servicio.EliminarAsync(nivel.Id);

        _repositorio.Verify(r => r.Eliminar(nivel), Times.Once);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }
}
