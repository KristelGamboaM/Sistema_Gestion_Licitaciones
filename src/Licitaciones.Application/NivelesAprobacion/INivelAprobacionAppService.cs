namespace Licitaciones.Application.NivelesAprobacion;

public interface INivelAprobacionAppService
{
    Task<NivelAprobacionDto> CrearAsync(
        GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken = default);

    Task<NivelAprobacionDto> ActualizarAsync(
        Guid id, GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NivelAprobacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NivelAprobacionDto>> ListarAsync(CancellationToken cancellationToken = default);
}
