using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Domain.Servicios;

namespace Licitaciones.Application.Proveedores;

public sealed class ProveedorAppService(
    IProveedorRepository repositorio, IUnitOfWork unitOfWork, IReloj reloj) : IProveedorAppService
{
    public async Task<ProveedorDto> CrearAsync(CrearProveedorRequest solicitud, CancellationToken cancellationToken = default)
    {
        await ValidarNombreUnicoAsync(solicitud.Nombre, excluirId: null, cancellationToken);

        var proveedor = Proveedor.Crear(solicitud.Nombre, reloj);
        repositorio.Agregar(proveedor);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(proveedor);
    }

    public async Task<ProveedorDto> ActualizarAsync(
        Guid id, ActualizarProveedorRequest solicitud, CancellationToken cancellationToken = default)
    {
        var proveedor = await ObtenerEntidadAsync(id, cancellationToken);
        await ValidarNombreUnicoAsync(solicitud.Nombre, excluirId: id, cancellationToken);

        proveedor.Actualizar(solicitud.Nombre, reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(proveedor);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await ObtenerEntidadAsync(id, cancellationToken);
        proveedor.Eliminar(reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<ProveedorDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default) =>
        AMapa(await ObtenerEntidadAsync(id, cancellationToken));

    public async Task<PaginaResultado<ProveedorDto>> ListarAsync(
        ConsultaProveedores consulta, CancellationToken cancellationToken = default)
    {
        var pagina = await repositorio.ListarAsync(consulta, cancellationToken);
        return new PaginaResultado<ProveedorDto>(
            pagina.Elementos.Select(AMapa).ToList(), pagina.Total, pagina.Pagina, pagina.TamanoPagina);
    }

    private async Task<Proveedor> ObtenerEntidadAsync(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await repositorio.ObtenerPorIdAsync(id, cancellationToken);
        if (proveedor is null || proveedor.EstaEliminado)
            throw new EntidadNoEncontradaException("Proveedor", id);

        return proveedor;
    }

    private async Task ValidarNombreUnicoAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken)
    {
        var nombreNormalizado = NormalizacionTexto.NormalizarNombreProveedor(nombre.Trim());
        var existe = await repositorio.ExisteNombreNormalizadoAsync(nombreNormalizado, excluirId, cancellationToken);
        if (existe)
        {
            throw new ReglaNegocioException(
                "proveedor.nombre.duplicado",
                "Ya existe un proveedor registrado con ese nombre.",
                TipoErrorNegocio.Conflicto);
        }
    }

    private static ProveedorDto AMapa(Proveedor proveedor) =>
        new(proveedor.Id, proveedor.Nombre, proveedor.CreatedAt, proveedor.UpdatedAt);
}
