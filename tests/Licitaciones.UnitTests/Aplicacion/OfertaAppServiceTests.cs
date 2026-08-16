using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;
using Moq;

namespace Licitaciones.UnitTests.Aplicacion;

public class OfertaAppServiceTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private readonly Mock<IOfertaRepository> _repositorio = new();
    private readonly Mock<ILicitacionRepository> _licitacionRepositorio = new();
    private readonly Mock<IProveedorRepository> _proveedorRepositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OfertaAppService _servicio;

    private readonly Licitacion _licitacionPublicada;
    private readonly Proveedor _proveedor;

    public OfertaAppServiceTests()
    {
        _servicio = new OfertaAppService(
            _repositorio.Object, _licitacionRepositorio.Object, _proveedorRepositorio.Object,
            _unitOfWork.Object, Reloj);

        _licitacionPublicada = Licitacion.Crear("LIC-1", "Título", 1_000_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        _licitacionPublicada.Publicar(Reloj);
        _proveedor = Proveedor.Crear("Proveedor Uno", Reloj);

        _licitacionRepositorio.Setup(r => r.ObtenerPorIdAsync(_licitacionPublicada.Id, default))
            .ReturnsAsync(_licitacionPublicada);
        _proveedorRepositorio.Setup(r => r.ObtenerPorIdAsync(_proveedor.Id, default)).ReturnsAsync(_proveedor);
    }

    [Fact]
    public async Task RegistrarAsync_conDatosValidos_agregaYGuarda()
    {
        _repositorio.Setup(r => r.ExisteOfertaDeProveedorAsync(_licitacionPublicada.Id, _proveedor.Id, default))
            .ReturnsAsync(false);

        var resultado = await _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(_licitacionPublicada.Id, _proveedor.Id, 500_000m));

        Assert.Equal("Proveedor Uno", resultado.ProveedorNombre);
        _repositorio.Verify(r => r.Agregar(It.IsAny<Oferta>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_conOfertaDuplicada_lanzaConflicto()
    {
        _repositorio.Setup(r => r.ExisteOfertaDeProveedorAsync(_licitacionPublicada.Id, _proveedor.Id, default))
            .ReturnsAsync(true);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(_licitacionPublicada.Id, _proveedor.Id, 500_000m)));

        Assert.Equal("oferta.duplicada", excepcion.Codigo);
        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
    }

    [Fact]
    public async Task RegistrarAsync_conMontoSuperiorAlPresupuesto_lanzaReglaNegocio()
    {
        _repositorio.Setup(r => r.ExisteOfertaDeProveedorAsync(_licitacionPublicada.Id, _proveedor.Id, default))
            .ReturnsAsync(false);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(_licitacionPublicada.Id, _proveedor.Id, 2_000_000m)));

        Assert.Equal("oferta.monto.excede_presupuesto", excepcion.Codigo);
    }

    [Fact]
    public async Task RegistrarAsync_montoIgualAlPresupuesto_esValido()
    {
        _repositorio.Setup(r => r.ExisteOfertaDeProveedorAsync(_licitacionPublicada.Id, _proveedor.Id, default))
            .ReturnsAsync(false);

        var resultado = await _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(_licitacionPublicada.Id, _proveedor.Id, 1_000_000m));

        Assert.Equal(1_000_000m, resultado.MontoOfertadoCRC);
    }

    [Fact]
    public async Task RegistrarAsync_sobreLicitacionEnBorrador_lanzaReglaNegocio()
    {
        var licitacionBorrador = Licitacion.Crear("LIC-2", "Título", 100m, Reloj.UtcAhora.AddDays(5), Reloj);
        _licitacionRepositorio.Setup(r => r.ObtenerPorIdAsync(licitacionBorrador.Id, default))
            .ReturnsAsync(licitacionBorrador);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(licitacionBorrador.Id, _proveedor.Id, 50m)));

        Assert.Equal("oferta.licitacion_no_publicada", excepcion.Codigo);
    }

    [Fact]
    public async Task RegistrarAsync_sobreLicitacionVencidaPorFecha_lanzaReglaNegocio()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var licitacion = Licitacion.Crear("LIC-3", "Título", 100m, reloj.UtcAhora.AddDays(1), reloj);
        licitacion.Publicar(reloj);
        reloj.AvanzarA(reloj.UtcAhora.AddDays(5)); // fecha de cierre ya pasó

        var servicio = new OfertaAppService(
            _repositorio.Object, _licitacionRepositorio.Object, _proveedorRepositorio.Object, _unitOfWork.Object, reloj);
        _licitacionRepositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.RegistrarAsync(
            new RegistrarOfertaRequest(licitacion.Id, _proveedor.Id, 50m)));

        Assert.Equal("oferta.licitacion_cerrada", excepcion.Codigo);
    }

    [Fact]
    public async Task RegistrarAsync_conProveedorInexistente_lanzaEntidadNoEncontrada()
    {
        _proveedorRepositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Proveedor?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.RegistrarAsync(
            new RegistrarOfertaRequest(_licitacionPublicada.Id, Guid.NewGuid(), 50m)));
    }

    [Fact]
    public async Task ActualizarAsync_conMontoValido_actualizaYGuarda()
    {
        var oferta = Oferta.Registrar(_licitacionPublicada.Id, _proveedor.Id, 400_000m, Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(oferta.Id, default)).ReturnsAsync(oferta);

        var resultado = await _servicio.ActualizarAsync(oferta.Id, new ActualizarOfertaRequest(600_000m));

        Assert.Equal(600_000m, resultado.MontoOfertadoCRC);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_conMontoSuperiorAlPresupuesto_lanzaReglaNegocio()
    {
        var oferta = Oferta.Registrar(_licitacionPublicada.Id, _proveedor.Id, 400_000m, Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(oferta.Id, default)).ReturnsAsync(oferta);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => _servicio.ActualizarAsync(oferta.Id, new ActualizarOfertaRequest(5_000_000m)));

        Assert.Equal("oferta.monto.excede_presupuesto", excepcion.Codigo);
    }

    [Fact]
    public async Task ActualizarAsync_sobreLicitacionCerrada_lanzaReglaNegocio()
    {
        var reloj = RelojFalso.EnUtc(2026, 1, 10);
        var licitacion = Licitacion.Crear("LIC-4", "Título", 100m, reloj.UtcAhora.AddDays(1), reloj);
        licitacion.Publicar(reloj);
        var oferta = Oferta.Registrar(licitacion.Id, _proveedor.Id, 50m, reloj);
        reloj.AvanzarA(reloj.UtcAhora.AddDays(5));

        var servicio = new OfertaAppService(
            _repositorio.Object, _licitacionRepositorio.Object, _proveedorRepositorio.Object, _unitOfWork.Object, reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(oferta.Id, default)).ReturnsAsync(oferta);
        _licitacionRepositorio.Setup(r => r.ObtenerPorIdAsync(licitacion.Id, default)).ReturnsAsync(licitacion);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => servicio.ActualizarAsync(oferta.Id, new ActualizarOfertaRequest(80m)));

        Assert.Equal("oferta.licitacion_cerrada", excepcion.Codigo);
    }

    [Fact]
    public async Task ActualizarAsync_conIdInexistente_lanzaEntidadNoEncontrada()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Oferta?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(
            () => _servicio.ActualizarAsync(Guid.NewGuid(), new ActualizarOfertaRequest(100m)));
    }

    [Fact]
    public async Task EliminarAsync_conOfertaSobreLicitacionActiva_eliminaYGuarda()
    {
        var oferta = Oferta.Registrar(_licitacionPublicada.Id, _proveedor.Id, 400_000m, Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(oferta.Id, default)).ReturnsAsync(oferta);

        await _servicio.EliminarAsync(oferta.Id);

        _repositorio.Verify(r => r.Eliminar(oferta), Times.Once);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ObtenerAsync_devuelveElDtoConNombreDeProveedor()
    {
        var oferta = Oferta.Registrar(_licitacionPublicada.Id, _proveedor.Id, 400_000m, Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(oferta.Id, default)).ReturnsAsync(oferta);

        var resultado = await _servicio.ObtenerAsync(oferta.Id);

        Assert.Equal("Proveedor Uno", resultado.ProveedorNombre);
    }

    [Fact]
    public async Task ListarAsync_mapeaLaPaginaDeEntidadesConNombreDeProveedor()
    {
        var oferta = Oferta.Registrar(_licitacionPublicada.Id, _proveedor.Id, 400_000m, Reloj);
        var consulta = new ConsultaOfertas(_licitacionPublicada.Id);
        _repositorio.Setup(r => r.ListarAsync(consulta, default))
            .ReturnsAsync(new Licitaciones.Domain.Comun.PaginaResultado<Oferta>([oferta], 1, 1, 20));

        var resultado = await _servicio.ListarAsync(consulta);

        Assert.Single(resultado.Elementos);
        Assert.Equal("Proveedor Uno", resultado.Elementos[0].ProveedorNombre);
    }
}
