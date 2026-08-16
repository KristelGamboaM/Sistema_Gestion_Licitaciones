using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositorios;

public sealed class ProveedorRepository(LicitacionesDbContext contexto) : IProveedorRepository
{
    public Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Proveedores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado, Guid? excluirId = null, CancellationToken cancellationToken = default)
    {
        var consulta = contexto.Proveedores.Where(p => p.NombreNormalizado == nombreNormalizado);
        if (excluirId is not null)
            consulta = consulta.Where(p => p.Id != excluirId);

        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task<PaginaResultado<Proveedor>> ListarAsync(
        ConsultaProveedores consulta, CancellationToken cancellationToken = default)
    {
        var query = contexto.Proveedores.AsQueryable();

        if (!consulta.IncluirEliminados)
            query = query.Where(p => p.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(consulta.Busqueda))
        {
            var termino = consulta.Busqueda.Trim().ToUpperInvariant();
            query = query.Where(p => p.NombreNormalizado.Contains(termino));
        }

        query = consulta.OrdenarDescendente
            ? query.OrderByDescending(p => p.Nombre)
            : query.OrderBy(p => p.Nombre);

        var total = await query.CountAsync(cancellationToken);
        var elementos = await query
            .Skip((consulta.Pagina - 1) * consulta.TamanoPagina)
            .Take(consulta.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new PaginaResultado<Proveedor>(elementos, total, consulta.Pagina, consulta.TamanoPagina);
    }

    public void Agregar(Proveedor proveedor) => contexto.Proveedores.Add(proveedor);
}
