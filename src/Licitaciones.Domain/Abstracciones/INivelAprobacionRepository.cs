using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Abstracciones;

public interface INivelAprobacionRepository
{
    Task<NivelAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NivelAprobacion>> ListarTodosAsync(CancellationToken cancellationToken = default);

    void Agregar(NivelAprobacion nivel);

    void Eliminar(NivelAprobacion nivel);
}
