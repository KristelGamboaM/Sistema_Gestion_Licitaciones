using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;
using Moq;

namespace Licitaciones.UnitTests.Aplicacion;

public class ProveedorAppServiceTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    private readonly Mock<IProveedorRepository> _repositorio = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ProveedorAppService _servicio;

    public ProveedorAppServiceTests()
    {
        _servicio = new ProveedorAppService(_repositorio.Object, _unitOfWork.Object, Reloj);
    }

    [Fact]
    public async Task CrearAsync_conNombreNoDuplicado_agregaYGuarda()
    {
        _repositorio.Setup(r => r.ExisteNombreNormalizadoAsync("EMPRESA CENTRAL", null, default))
            .ReturnsAsync(false);

        var resultado = await _servicio.CrearAsync(new CrearProveedorRequest("Empresa Central"));

        Assert.Equal("Empresa Central", resultado.Nombre);
        _repositorio.Verify(r => r.Agregar(It.IsAny<Proveedor>()), Times.Once);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_conNombreDuplicado_lanzaConflictoYNoGuarda()
    {
        _repositorio.Setup(r => r.ExisteNombreNormalizadoAsync("EMPRESA CENTRAL", null, default))
            .ReturnsAsync(true);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => _servicio.CrearAsync(new CrearProveedorRequest("Empresa Central")));

        Assert.Equal("proveedor.nombre.duplicado", excepcion.Codigo);
        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Never);
    }

    [Fact]
    public async Task ObtenerAsync_conIdInexistente_lanzaEntidadNoEncontrada()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Proveedor?)null);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.ObtenerAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerAsync_conProveedorEliminado_lanzaEntidadNoEncontrada()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);
        proveedor.Eliminar(Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default)).ReturnsAsync(proveedor);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => _servicio.ObtenerAsync(proveedor.Id));
    }

    [Fact]
    public async Task EliminarAsync_marcaBorradoLogicoYGuarda()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);
        _repositorio.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default)).ReturnsAsync(proveedor);

        await _servicio.EliminarAsync(proveedor.Id);

        Assert.True(proveedor.EstaEliminado);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task ListarAsync_mapeaLaPaginaDeEntidadesADto()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);
        var consulta = new ConsultaProveedores();
        _repositorio.Setup(r => r.ListarAsync(consulta, default))
            .ReturnsAsync(new PaginaResultado<Proveedor>([proveedor], 1, 1, 20));

        var resultado = await _servicio.ListarAsync(consulta);

        Assert.Single(resultado.Elementos);
        Assert.Equal(proveedor.Nombre, resultado.Elementos[0].Nombre);
    }
}
