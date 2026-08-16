using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositorios;

public sealed class LicitacionRepository(LicitacionesDbContext contexto) : ILicitacionRepository
{
    public Task<Licitacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Licitaciones.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<bool> ExisteCodigoNormalizadoAsync(
        string codigoNormalizado, Guid? excluirId = null, CancellationToken cancellationToken = default)
    {
        var consulta = contexto.Licitaciones.Where(l => l.CodigoNormalizado == codigoNormalizado);
        if (excluirId is not null)
            consulta = consulta.Where(l => l.Id != excluirId);

        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task<PaginaResultado<Licitacion>> ListarAsync(
        ConsultaLicitaciones consulta, CancellationToken cancellationToken = default)
    {
        var query = contexto.Licitaciones.AsQueryable();

        if (!consulta.IncluirEliminadas)
            query = query.Where(l => l.DeletedAt == null);

        if (consulta.Estado is not null)
            query = query.Where(l => l.Estado == consulta.Estado);

        if (!string.IsNullOrWhiteSpace(consulta.Busqueda))
        {
            var termino = consulta.Busqueda.Trim().ToUpperInvariant();
            query = query.Where(l => l.CodigoNormalizado.Contains(termino) || l.Titulo.ToUpper().Contains(termino));
        }

        query = (consulta.OrdenarPor, consulta.OrdenarDescendente) switch
        {
            (ColumnaOrdenLicitacion.Codigo, true) => query.OrderByDescending(l => l.CodigoNormalizado),
            (ColumnaOrdenLicitacion.Codigo, false) => query.OrderBy(l => l.CodigoNormalizado),
            (ColumnaOrdenLicitacion.Titulo, true) => query.OrderByDescending(l => l.Titulo),
            (ColumnaOrdenLicitacion.Titulo, false) => query.OrderBy(l => l.Titulo),
            (ColumnaOrdenLicitacion.Presupuesto, true) => query.OrderByDescending(l => l.PresupuestoEstimadoCRC),
            (ColumnaOrdenLicitacion.Presupuesto, false) => query.OrderBy(l => l.PresupuestoEstimadoCRC),
            (ColumnaOrdenLicitacion.Estado, true) => query.OrderByDescending(l => l.Estado),
            (ColumnaOrdenLicitacion.Estado, false) => query.OrderBy(l => l.Estado),
            (_, true) => query.OrderByDescending(l => l.FechaCierre),
            (_, false) => query.OrderBy(l => l.FechaCierre),
        };

        var total = await query.CountAsync(cancellationToken);
        var elementos = await query
            .Skip((consulta.Pagina - 1) * consulta.TamanoPagina)
            .Take(consulta.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new PaginaResultado<Licitacion>(elementos, total, consulta.Pagina, consulta.TamanoPagina);
    }

    public void Agregar(Licitacion licitacion) => contexto.Licitaciones.Add(licitacion);
}
