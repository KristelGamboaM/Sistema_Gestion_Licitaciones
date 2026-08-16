using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorAppService
{
    Task<ProveedorDto> CrearAsync(CrearProveedorRequest solicitud, CancellationToken cancellationToken = default);

    Task<ProveedorDto> ActualizarAsync(
        Guid id, ActualizarProveedorRequest solicitud, CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProveedorDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaginaResultado<ProveedorDto>> ListarAsync(
        ConsultaProveedores consulta, CancellationToken cancellationToken = default);
}
