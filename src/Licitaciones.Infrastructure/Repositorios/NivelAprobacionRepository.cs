using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositorios;

public sealed class NivelAprobacionRepository(LicitacionesDbContext contexto) : INivelAprobacionRepository
{
    public Task<NivelAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.NivelesAprobacion.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<NivelAprobacion>> ListarTodosAsync(CancellationToken cancellationToken = default) =>
        await contexto.NivelesAprobacion.OrderBy(n => n.MontoMinimoCRC).ToListAsync(cancellationToken);

    public void Agregar(NivelAprobacion nivel) => contexto.NivelesAprobacion.Add(nivel);

    public void Eliminar(NivelAprobacion nivel) => contexto.NivelesAprobacion.Remove(nivel);
}
