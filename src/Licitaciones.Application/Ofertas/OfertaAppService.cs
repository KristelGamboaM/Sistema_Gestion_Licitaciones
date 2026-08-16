using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Application.Ofertas;

public sealed class OfertaAppService(
    IOfertaRepository repositorio,
    ILicitacionRepository licitacionRepositorio,
    IProveedorRepository proveedorRepositorio,
    IUnitOfWork unitOfWork,
    IReloj reloj) : IOfertaAppService
{
    public async Task<OfertaDto> RegistrarAsync(RegistrarOfertaRequest solicitud, CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerLicitacionDisponibleAsync(solicitud.LicitacionId, cancellationToken);

        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(solicitud.ProveedorId, cancellationToken);
        if (proveedor is null || proveedor.EstaEliminado)
            throw new EntidadNoEncontradaException("Proveedor", solicitud.ProveedorId);

        var yaOferto = await repositorio.ExisteOfertaDeProveedorAsync(
            solicitud.LicitacionId, solicitud.ProveedorId, cancellationToken);
        if (yaOferto)
        {
            throw new ReglaNegocioException(
                "oferta.duplicada",
                "El proveedor ya registró una oferta para esta licitación.",
                TipoErrorNegocio.Conflicto);
        }

        ValidarMontoNoSuperaPresupuesto(solicitud.MontoOfertadoCRC, licitacion.PresupuestoEstimadoCRC);

        var oferta = Oferta.Registrar(solicitud.LicitacionId, solicitud.ProveedorId, solicitud.MontoOfertadoCRC, reloj);
        repositorio.Agregar(oferta);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(oferta, proveedor.Nombre);
    }

    public async Task<OfertaDto> ActualizarAsync(
        Guid id, ActualizarOfertaRequest solicitud, CancellationToken cancellationToken = default)
    {
        var oferta = await ObtenerEntidadAsync(id, cancellationToken);
        var licitacion = await ObtenerLicitacionDisponibleAsync(oferta.LicitacionId, cancellationToken);
        ValidarMontoNoSuperaPresupuesto(solicitud.MontoOfertadoCRC, licitacion.PresupuestoEstimadoCRC);

        oferta.ActualizarMonto(solicitud.MontoOfertadoCRC, reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
        return AMapa(oferta, proveedor?.Nombre ?? string.Empty);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var oferta = await ObtenerEntidadAsync(id, cancellationToken);
        await ObtenerLicitacionDisponibleAsync(oferta.LicitacionId, cancellationToken, permitirBorrador: true);

        // No hay borrado lógico específico para Oferta (spec §7); al no permitirse
        // sobre licitaciones cerradas, se elimina físicamente solo mientras la
        // licitación sigue activa. El repositorio se apoya en el DbContext.
        repositorio.Eliminar(oferta);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<OfertaDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var oferta = await ObtenerEntidadAsync(id, cancellationToken);
        var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
        return AMapa(oferta, proveedor?.Nombre ?? string.Empty);
    }

    public async Task<PaginaResultado<OfertaDto>> ListarAsync(
        ConsultaOfertas consulta, CancellationToken cancellationToken = default)
    {
        var pagina = await repositorio.ListarAsync(consulta, cancellationToken);
        var dtos = new List<OfertaDto>(pagina.Elementos.Count);
        foreach (var oferta in pagina.Elementos)
        {
            var proveedor = await proveedorRepositorio.ObtenerPorIdAsync(oferta.ProveedorId, cancellationToken);
            dtos.Add(AMapa(oferta, proveedor?.Nombre ?? string.Empty));
        }

        return new PaginaResultado<OfertaDto>(dtos, pagina.Total, pagina.Pagina, pagina.TamanoPagina);
    }

    private async Task<Oferta> ObtenerEntidadAsync(Guid id, CancellationToken cancellationToken)
    {
        var oferta = await repositorio.ObtenerPorIdAsync(id, cancellationToken);
        if (oferta is null)
            throw new EntidadNoEncontradaException("Oferta", id);

        return oferta;
    }

    /// <param name="permitirBorrador">
    /// La eliminación reutiliza esta validación de "licitación disponible";
    /// en la práctica una oferta nunca existe sobre una licitación en
    /// Borrador, pero se deja explícito para no acoplar registro y borrado.
    /// </param>
    private async Task<Licitacion> ObtenerLicitacionDisponibleAsync(
        Guid licitacionId, CancellationToken cancellationToken, bool permitirBorrador = false)
    {
        var licitacion = await licitacionRepositorio.ObtenerPorIdAsync(licitacionId, cancellationToken);
        if (licitacion is null || licitacion.EstaEliminada)
            throw new EntidadNoEncontradaException("Licitación", licitacionId);

        if (licitacion.EsEfectivamenteCerrada(reloj))
        {
            throw new ReglaNegocioException(
                "oferta.licitacion_cerrada",
                "No se pueden crear, editar ni eliminar ofertas de una licitación cerrada o vencida.",
                TipoErrorNegocio.Conflicto);
        }

        if (!permitirBorrador && licitacion.Estado != EstadoLicitacion.Publicada)
        {
            throw new ReglaNegocioException(
                "oferta.licitacion_no_publicada",
                "Solo se pueden registrar ofertas sobre licitaciones publicadas.",
                TipoErrorNegocio.Conflicto);
        }

        return licitacion;
    }

    private static void ValidarMontoNoSuperaPresupuesto(decimal montoOfertadoCRC, decimal presupuestoEstimadoCRC)
    {
        if (montoOfertadoCRC > presupuestoEstimadoCRC)
        {
            throw new ReglaNegocioException(
                "oferta.monto.excede_presupuesto",
                "El monto ofertado no puede superar el presupuesto estimado de la licitación.");
        }
    }

    private static OfertaDto AMapa(Oferta oferta, string proveedorNombre) => new(
        oferta.Id, oferta.LicitacionId, oferta.ProveedorId, proveedorNombre,
        oferta.MontoOfertadoCRC, oferta.FechaRegistro, oferta.UpdatedAt);
}
