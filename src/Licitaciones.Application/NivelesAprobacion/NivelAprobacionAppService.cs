using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Application.NivelesAprobacion;

public sealed class NivelAprobacionAppService(
    INivelAprobacionRepository repositorio, IUnitOfWork unitOfWork, IReloj reloj) : INivelAprobacionAppService
{
    public async Task<NivelAprobacionDto> CrearAsync(
        GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken = default)
    {
        var nivel = NivelAprobacion.Crear(solicitud.MontoMinimoCRC, solicitud.MontoMaximoCRC, solicitud.Aprobador, reloj);
        await ValidarSinTraslapeAsync(nivel, excluirId: null, cancellationToken);

        repositorio.Agregar(nivel);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(nivel);
    }

    public async Task<NivelAprobacionDto> ActualizarAsync(
        Guid id, GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken = default)
    {
        var nivel = await ObtenerEntidadAsync(id, cancellationToken);
        nivel.Actualizar(solicitud.MontoMinimoCRC, solicitud.MontoMaximoCRC, solicitud.Aprobador, reloj);
        await ValidarSinTraslapeAsync(nivel, excluirId: id, cancellationToken);

        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(nivel);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var nivel = await ObtenerEntidadAsync(id, cancellationToken);
        repositorio.Eliminar(nivel);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<NivelAprobacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default) =>
        AMapa(await ObtenerEntidadAsync(id, cancellationToken));

    public async Task<IReadOnlyList<NivelAprobacionDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var niveles = await repositorio.ListarTodosAsync(cancellationToken);
        return niveles.Select(AMapa).ToList();
    }

    private async Task<NivelAprobacion> ObtenerEntidadAsync(Guid id, CancellationToken cancellationToken)
    {
        var nivel = await repositorio.ObtenerPorIdAsync(id, cancellationToken);
        if (nivel is null)
            throw new EntidadNoEncontradaException("Nivel de aprobación", id);

        return nivel;
    }

    private async Task ValidarSinTraslapeAsync(NivelAprobacion nivel, Guid? excluirId, CancellationToken cancellationToken)
    {
        var existentes = await repositorio.ListarTodosAsync(cancellationToken);
        var traslapa = existentes.Any(existente => existente.Id != excluirId && existente.SeTraslapaCon(nivel));

        if (traslapa)
        {
            throw new ReglaNegocioException(
                "nivel.rango.traslape",
                "El rango se traslapa con un nivel de aprobación ya configurado.",
                TipoErrorNegocio.Conflicto);
        }
    }

    private static NivelAprobacionDto AMapa(NivelAprobacion nivel) => new(
        nivel.Id, nivel.MontoMinimoCRC, nivel.MontoMaximoCRC, nivel.Aprobador, nivel.EsRangoAbierto,
        nivel.CreatedAt, nivel.UpdatedAt);
}
