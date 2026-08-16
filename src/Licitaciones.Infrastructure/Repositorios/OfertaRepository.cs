using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Repositorios;

public sealed class OfertaRepository(LicitacionesDbContext contexto) : IOfertaRepository
{
    public Task<Oferta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Ofertas.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<bool> ExisteOfertaDeProveedorAsync(
        Guid licitacionId, Guid proveedorId, CancellationToken cancellationToken = default) =>
        contexto.Ofertas.AnyAsync(o => o.LicitacionId == licitacionId && o.ProveedorId == proveedorId, cancellationToken);

    public async Task<IReadOnlyList<Oferta>> ListarPorLicitacionAsync(
        Guid licitacionId, CancellationToken cancellationToken = default) =>
        await contexto.Ofertas
            .Where(o => o.LicitacionId == licitacionId)
            .OrderBy(o => o.MontoOfertadoCRC)
            .ThenBy(o => o.FechaRegistro)
            .ToListAsync(cancellationToken);

    public async Task<decimal?> ObtenerMontoMayorAsync(Guid licitacionId, CancellationToken cancellationToken = default)
    {
        var ofertas = contexto.Ofertas.Where(o => o.LicitacionId == licitacionId);
        return await ofertas.AnyAsync(cancellationToken)
            ? await ofertas.MaxAsync(o => o.MontoOfertadoCRC, cancellationToken)
            : null;
    }

    public async Task<PaginaResultado<Oferta>> ListarAsync(
        ConsultaOfertas consulta, CancellationToken cancellationToken = default)
    {
        var query = contexto.Ofertas.AsQueryable();

        if (consulta.LicitacionId is not null)
            query = query.Where(o => o.LicitacionId == consulta.LicitacionId);

        if (consulta.ProveedorId is not null)
            query = query.Where(o => o.ProveedorId == consulta.ProveedorId);

        query = (consulta.OrdenarPor, consulta.OrdenarDescendente) switch
        {
            (ColumnaOrdenOferta.Monto, true) => query.OrderByDescending(o => o.MontoOfertadoCRC),
            (ColumnaOrdenOferta.Monto, false) => query.OrderBy(o => o.MontoOfertadoCRC),
            (_, true) => query.OrderByDescending(o => o.FechaRegistro),
            (_, false) => query.OrderBy(o => o.FechaRegistro),
        };

        var total = await query.CountAsync(cancellationToken);
        var elementos = await query
            .Skip((consulta.Pagina - 1) * consulta.TamanoPagina)
            .Take(consulta.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new PaginaResultado<Oferta>(elementos, total, consulta.Pagina, consulta.TamanoPagina);
    }

    public void Agregar(Oferta oferta) => contexto.Ofertas.Add(oferta);

    public void Eliminar(Oferta oferta) => contexto.Ofertas.Remove(oferta);
}
