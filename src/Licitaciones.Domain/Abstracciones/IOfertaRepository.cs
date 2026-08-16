using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Abstracciones;

public enum ColumnaOrdenOferta
{
    FechaRegistro,
    Monto,
}

public sealed record ConsultaOfertas(
    Guid? LicitacionId = null,
    Guid? ProveedorId = null,
    int Pagina = 1,
    int TamanoPagina = 20,
    bool OrdenarDescendente = true,
    ColumnaOrdenOferta OrdenarPor = ColumnaOrdenOferta.FechaRegistro);

public interface IOfertaRepository
{
    Task<Oferta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteOfertaDeProveedorAsync(
        Guid licitacionId, Guid proveedorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Oferta>> ListarPorLicitacionAsync(
        Guid licitacionId, CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerMontoMayorAsync(Guid licitacionId, CancellationToken cancellationToken = default);

    Task<PaginaResultado<Oferta>> ListarAsync(
        ConsultaOfertas consulta, CancellationToken cancellationToken = default);

    void Agregar(Oferta oferta);

    void Eliminar(Oferta oferta);
}
