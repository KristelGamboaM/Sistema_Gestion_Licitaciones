using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Abstracciones;

namespace Licitaciones.Web.Models.Ofertas;

public sealed class OfertaIndexViewModel
{
    public IReadOnlyList<OfertaDto> Ofertas { get; init; } = [];
    public Guid? LicitacionId { get; init; }
    public string? LicitacionCodigo { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanoPagina { get; init; } = 20;
    public int Total { get; init; }
    public ColumnaOrdenOferta OrdenarPor { get; init; } = ColumnaOrdenOferta.FechaRegistro;
    public bool Descendente { get; init; } = true;

    public int TotalPaginas => TamanoPagina == 0 ? 1 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}
