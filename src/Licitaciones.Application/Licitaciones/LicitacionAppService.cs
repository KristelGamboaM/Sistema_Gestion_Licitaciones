using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Domain.Servicios;

namespace Licitaciones.Application.Licitaciones;

public sealed class LicitacionAppService(
    ILicitacionRepository repositorio,
    IOfertaRepository ofertaRepositorio,
    INivelAprobacionRepository nivelAprobacionRepositorio,
    IProveedorRepository proveedorRepositorio,
    IUnitOfWork unitOfWork,
    IReloj reloj) : ILicitacionAppService
{
    public async Task<LicitacionDto> CrearAsync(CrearLicitacionRequest solicitud, CancellationToken cancellationToken = default)
    {
        await ValidarCodigoUnicoAsync(solicitud.Codigo, excluirId: null, cancellationToken);

        var licitacion = Licitacion.Crear(
            solicitud.Codigo, solicitud.Titulo, solicitud.PresupuestoEstimadoCRC, solicitud.FechaCierre, reloj);
        repositorio.Agregar(licitacion);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(licitacion);
    }

    public async Task<LicitacionDto> ActualizarAsync(
        Guid id, ActualizarLicitacionRequest solicitud, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerEntidadAsync(id, cancellationToken);
        await ValidarCodigoUnicoAsync(solicitud.Codigo, excluirId: id, cancellationToken);
        var montoMayorOferta = await ofertaRepositorio.ObtenerMontoMayorAsync(id, cancellationToken);

        licitacion.ActualizarDatosBorrador(
            solicitud.Codigo, solicitud.Titulo, solicitud.PresupuestoEstimadoCRC, solicitud.FechaCierre,
            montoMayorOferta, reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(licitacion);
    }

    public async Task<LicitacionDto> PublicarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerEntidadAsync(id, cancellationToken);
        licitacion.Publicar(reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return AMapa(licitacion);
    }

    public async Task<LicitacionDto> CerrarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerEntidadAsync(id, cancellationToken);
        licitacion.Cerrar(reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
        return AMapa(licitacion);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerEntidadAsync(id, cancellationToken);
        licitacion.Eliminar(reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<LicitacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default) =>
        AMapa(await ObtenerEntidadAsync(id, cancellationToken));

    public async Task<PaginaResultado<LicitacionDto>> ListarAsync(
        ConsultaLicitaciones consulta, CancellationToken cancellationToken = default)
    {
        var pagina = await repositorio.ListarAsync(consulta, cancellationToken);
        return new PaginaResultado<LicitacionDto>(
            pagina.Elementos.Select(AMapa).ToList(), pagina.Total, pagina.Pagina, pagina.TamanoPagina);
    }

    public async Task<MejorOfertaDto> ObtenerMejorOfertaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerEntidadAsync(id, cancellationToken);
        var ofertas = await ofertaRepositorio.ListarPorLicitacionAsync(id, cancellationToken);
        var resultado = CalculadoraMejorOferta.Calcular(licitacion.PresupuestoEstimadoCRC, ofertas);

        string? aprobador = null;
        string? proveedorNombre = null;
        if (resultado.Mejor is not null)
        {
            var niveles = await nivelAprobacionRepositorio.ListarTodosAsync(cancellationToken);
            aprobador = ResolutorNivelAprobacion.Resolver(niveles, licitacion.PresupuestoEstimadoCRC)?.Aprobador;

            var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(resultado.Mejor.ProveedorId, cancellationToken);
            proveedorNombre = proveedor?.Nombre;
        }

        return new MejorOfertaDto(
            resultado.Mejor is not null,
            resultado.Mejor?.ProveedorId,
            proveedorNombre,
            resultado.Mejor?.MontoOfertadoCRC,
            resultado.PorcentajeAhorro,
            resultado.Clasificacion.AMensaje(),
            aprobador);
    }

    private async Task<Licitacion> ObtenerEntidadAsync(Guid id, CancellationToken cancellationToken)
    {
        var licitacion = await repositorio.ObtenerPorIdAsync(id, cancellationToken);
        if (licitacion is null || licitacion.EstaEliminada)
            throw new EntidadNoEncontradaException("Licitación", id);

        return licitacion;
    }

    private async Task ValidarCodigoUnicoAsync(string codigo, Guid? excluirId, CancellationToken cancellationToken)
    {
        var codigoNormalizado = NormalizacionTexto.NormalizarCodigoLicitacion(codigo);
        var existe = await repositorio.ExisteCodigoNormalizadoAsync(codigoNormalizado, excluirId, cancellationToken);
        if (existe)
        {
            throw new ReglaNegocioException(
                "licitacion.codigo.duplicado",
                "Ya existe una licitación registrada con ese código.",
                TipoErrorNegocio.Conflicto);
        }
    }

    private LicitacionDto AMapa(Licitacion licitacion) => new(
        licitacion.Id,
        licitacion.Codigo,
        licitacion.Titulo,
        licitacion.Estado.ToString(),
        licitacion.EsEfectivamenteCerrada(reloj),
        licitacion.FechaCierre,
        licitacion.PresupuestoEstimadoCRC,
        licitacion.CreatedAt,
        licitacion.UpdatedAt);
}
