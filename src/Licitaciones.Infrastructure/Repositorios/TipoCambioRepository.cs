using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositorios;

public sealed class TipoCambioRepository(LicitacionesDbContext contexto) : ITipoCambioRepository
{
    public Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.TiposCambio.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<TipoCambio?> ObtenerActivoAsync(CancellationToken cancellationToken = default) =>
        contexto.TiposCambio.FirstOrDefaultAsync(t => t.Activo, cancellationToken);

    public async Task<IReadOnlyList<TipoCambio>> ListarTodosAsync(CancellationToken cancellationToken = default) =>
        await contexto.TiposCambio.OrderByDescending(t => t.FechaVigencia).ToListAsync(cancellationToken);

    public void Agregar(TipoCambio tipoCambio) => contexto.TiposCambio.Add(tipoCambio);
}
