using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Abstracciones;

public interface ITipoCambioRepository
{
    Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TipoCambio?> ObtenerActivoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoCambio>> ListarTodosAsync(CancellationToken cancellationToken = default);

    void Agregar(TipoCambio tipoCambio);
}
