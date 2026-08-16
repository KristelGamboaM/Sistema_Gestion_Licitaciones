using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;

namespace Licitaciones.Application.Ofertas;

public interface IOfertaAppService
{
    Task<OfertaDto> RegistrarAsync(RegistrarOfertaRequest solicitud, CancellationToken cancellationToken = default);

    Task<OfertaDto> ActualizarAsync(
        Guid id, ActualizarOfertaRequest solicitud, CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OfertaDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaginaResultado<OfertaDto>> ListarAsync(ConsultaOfertas consulta, CancellationToken cancellationToken = default);
}
