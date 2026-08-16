using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;

namespace Licitaciones.Application.Licitaciones;

public interface ILicitacionAppService
{
    Task<LicitacionDto> CrearAsync(CrearLicitacionRequest solicitud, CancellationToken cancellationToken = default);

    Task<LicitacionDto> ActualizarAsync(
        Guid id, ActualizarLicitacionRequest solicitud, CancellationToken cancellationToken = default);

    Task<LicitacionDto> PublicarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LicitacionDto> CerrarAsync(Guid id, CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LicitacionDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaginaResultado<LicitacionDto>> ListarAsync(
        ConsultaLicitaciones consulta, CancellationToken cancellationToken = default);

    Task<MejorOfertaDto> ObtenerMejorOfertaAsync(Guid id, CancellationToken cancellationToken = default);
}
