namespace Licitaciones.Domain.Comun;

/// <summary>Resultado paginado genérico para listados de dominio (spec §10.2: paginación, filtrado y orden).</summary>
public sealed record PaginaResultado<T>(IReadOnlyList<T> Elementos, int Total, int Pagina, int TamanoPagina);
