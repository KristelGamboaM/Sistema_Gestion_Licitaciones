using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Abstracciones;

public sealed record ConsultaProveedores(
    string? Busqueda = null,
    bool IncluirEliminados = false,
    int Pagina = 1,
    int TamanoPagina = 20,
    bool OrdenarDescendente = false);

public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado, Guid? excluirId = null, CancellationToken cancellationToken = default);

    Task<PaginaResultado<Proveedor>> ListarAsync(
        ConsultaProveedores consulta, CancellationToken cancellationToken = default);

    void Agregar(Proveedor proveedor);
}
