using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Abstracciones;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class LicitacionIndexViewModel
{
    public IReadOnlyList<LicitacionDto> Licitaciones { get; init; } = [];
    public string? Busqueda { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanoPagina { get; init; } = 20;
    public int Total { get; init; }
    public ColumnaOrdenLicitacion OrdenarPor { get; init; } = ColumnaOrdenLicitacion.FechaCierre;
    public bool Descendente { get; init; }

    public int TotalPaginas => TamanoPagina == 0 ? 1 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}
