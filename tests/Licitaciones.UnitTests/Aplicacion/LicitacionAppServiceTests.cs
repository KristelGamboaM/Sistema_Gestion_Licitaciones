using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;
using Moq;

namespace Licitaciones.UnitTests.Aplicacion;

public class LicitacionAppServiceTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private readonly Mock<ILicitacionRepository> _repositorio = new();
    private readonly Mock<IOfertaRepository> _ofertaRepositorio = new();
    private readonly Mock<INivelAprobacionRepository> _nivelRepositorio = new();
    private readonly Mock<IProveedorRepository> _proveedorRepositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LicitacionAppService _servicio;

    public LicitacionAppServiceTests()
    {
        _servicio = new LicitacionAppService(
            _repositorio.Object, _ofertaRepositorio.Object, _nivelRepositorio.Object,
            _proveedorRepositorio.Object, _unitOfWork.Object, Reloj);
    }

    [Fact]
    public async Task CrearAsync_conCodigoNoDuplicado_agregaYGuarda()
    {
        _repositorio.Setup(r => r.ExisteCodigoNormalizadoAsync("LIC-2026-001", null, default)).ReturnsAsync(false);

        var resultado = await _servicio.CrearAsync(
            new CrearLicitacionRequest("LIC-2026-001", "Título", 1_000_000m, Reloj.UtcAhora.AddDays(10)));

        Assert.Equal("Borrador", resultado.Estado);
        _repositorio.Verify(r => r.Agregar(It.IsAny<Licitacion>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_conCodigoDuplicado_lanzaConflicto()
    {
        _repositorio.Setup(r => r.ExisteCodigoNormalizadoAsync("LIC-2026-001", null, default)).ReturnsAsync(true);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(
            new CrearLicitacionRequest("LIC-2026-001", "Título", 1_000_000m, Reloj.UtcAhora.AddDays(10))));

        Assert.Equal("licitacion.codigo.duplicado", excepcion.Codigo);
        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
    }

    [Fact]
    public async Task PublicarAsync_delegaEnElDominioYGuarda()
    {
        var licitacion = Licitacion.Crear("LIC-1", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);

        var resultado = await _servicio.PublicarAsync(licitacion.Id);

        Assert.Equal("Publicada", resultado.Estado);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ObtenerMejorOfertaAsync_sinOfertas_devuelveSinOfertaValida()
    {
        var licitacion = Licitacion.Crear("LIC-1", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);
        _ofertaRepositorio.Setup(r => r.ListarPorLicitacionAsync(licitacion.Id, default)).ReturnsAsync([]);

        var resultado = await _servicio.ObtenerMejorOfertaAsync(licitacion.Id);

        Assert.False(resultado.TieneOfertaValida);
        Assert.Equal("Sin ofertas válidas", resultado.Clasificacion);
    }

    [Fact]
    public async Task ObtenerMejorOfertaAsync_conOferta_resuelveAprobadorYNombreDeProveedor()
    {
        var licitacion = Licitacion.Crear("LIC-1", "Título", 900_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        var proveedor = Proveedor.Crear("Proveedor Uno", Reloj);
        var oferta = Oferta.Registrar(licitacion.Id, proveedor.Id, 800_000m, Reloj);
        var nivel = NivelAprobacion.Crear(0.01m, 999_999.99m, "Encargado de área", Reloj);

        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);
        _ofertaRepositorio.Setup(r => r.ListarPorLicitacionAsync(licitacion.Id, default)).ReturnsAsync([oferta]);
        _nivelRepositorio.Setup(r => r.ListarTodosAsync(default)).ReturnsAsync([nivel]);
        _proveedorRepositorio.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default)).ReturnsAsync(proveedor);

        var resultado = await _servicio.ObtenerMejorOfertaAsync(licitacion.Id);

        Assert.True(resultado.TieneOfertaValida);
        Assert.Equal("Proveedor Uno", resultado.ProveedorNombre);
        Assert.Equal("Encargado de área", resultado.Aprobador);
        Assert.Equal("Oferta conveniente", resultado.Clasificacion);
    }

    [Fact]
    public async Task ObtenerAsync_conIdInexistente_lanzaEntidadNoEncontrada()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Licitacion?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.ObtenerAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ActualizarAsync_enBorrador_actualizaYGuarda()
    {
        var licitacion = Licitacion.Crear("LIC-5", "Título", 100_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);
        _repositorio.Setup(r => r.ExisteCodigoNormalizadoAsync("LIC-6", licitacion.Id, default)).ReturnsAsync(false);
        _ofertaRepositorio.Setup(r => r.ObtenerMontoMayorAsync(licitacion.Id, default)).ReturnsAsync((decimal?)null);

        var resultado = await _servicio.ActualizarAsync(
            licitacion.Id, new ActualizarLicitacionRequest("LIC-6", "Otro título", 200_000m, Reloj.UtcAhora.AddDays(10)));

        Assert.Equal("LIC-6", resultado.Codigo);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_conCodigoDuplicado_lanzaConflicto()
    {
        var licitacion = Licitacion.Crear("LIC-7", "Título", 100_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);
        _repositorio.Setup(r => r.ExisteCodigoNormalizadoAsync("LIC-8", licitacion.Id, default)).ReturnsAsync(true);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.ActualizarAsync(
            licitacion.Id, new ActualizarLicitacionRequest("LIC-8", "Título", 100_000m, Reloj.UtcAhora.AddDays(5))));

        Assert.Equal("licitacion.codigo.duplicado", excepcion.Codigo);
    }

    [Fact]
    public async Task CerrarAsync_delegaEnElDominioYGuarda()
    {
        var licitacion = Licitacion.Crear("LIC-9", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);

        var resultado = await _servicio.CerrarAsync(licitacion.Id);

        Assert.Equal("Cerrada", resultado.Estado);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_marcaBorradoLogicoYGuarda()
    {
        var licitacion = Licitacion.Crear("LIC-10", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);

        await _servicio.EliminarAsync(licitacion.Id);

        Assert.True(licitacion.EstaEliminada);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ListarAsync_mapeaLaPaginaDeEntidadesADto()
    {
        var licitacion = Licitacion.Crear("LIC-11", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        var consulta = new ConsultaLicitaciones();
        _repositorio.Setup(r => r.ListarAsync(consulta, default))
            .ReturnsAsync(new Licitaciones.Domain.Comun.PaginaResultado<Licitacion>([licitacion], 1, 1, 20));

        var resultado = await _servicio.ListarAsync(consulta);

        Assert.Single(resultado.Elementos);
        Assert.Equal("LIC-11", resultado.Elementos[0].Codigo);
    }
}
