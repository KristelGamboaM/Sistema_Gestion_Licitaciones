using Licitaciones.Application.Proveedores;

namespace Licitaciones.Web.Models.Proveedores;

public sealed class ProveedorIndexViewModel
{
    public IReadOnlyList<ProveedorDto> Proveedores { get; init; } = [];
    public string? Busqueda { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanoPagina { get; init; } = 20;
    public int Total { get; init; }

    public int TotalPaginas => TamanoPagina == 0 ? 1 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}
