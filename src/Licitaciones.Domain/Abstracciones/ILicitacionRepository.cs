using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Entidades;

namespace Licitaciones.Domain.Abstracciones;

public enum ColumnaOrdenLicitacion
{
    FechaCierre,
    Codigo,
    Titulo,
    Presupuesto,
    Estado,
}

public sealed record ConsultaLicitaciones(
    string? Busqueda = null,
    EstadoLicitacion? Estado = null,
    bool IncluirEliminadas = false,
    int Pagina = 1,
    int TamanoPagina = 20,
    bool OrdenarDescendente = false,
    ColumnaOrdenLicitacion OrdenarPor = ColumnaOrdenLicitacion.FechaCierre);

public interface ILicitacionRepository
{
    Task<Licitacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoNormalizadoAsync(
        string codigoNormalizado, Guid? excluirId = null, CancellationToken cancellationToken = default);

    Task<PaginaResultado<Licitacion>> ListarAsync(
        ConsultaLicitaciones consulta, CancellationToken cancellationToken = default);

    void Agregar(Licitacion licitacion);
}
