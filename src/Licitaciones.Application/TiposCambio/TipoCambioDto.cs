namespace Licitaciones.Application.TiposCambio;

public sealed record TipoCambioDto(
    Guid Id,
    decimal CRCporUSD,
    DateTimeOffset FechaVigencia,
    bool Activo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record GuardarTipoCambioRequest(decimal CRCporUSD, DateTimeOffset FechaVigencia);

public sealed record ConversionDto(decimal MontoCRC, decimal MontoUSD, decimal CRCporUSD, DateTimeOffset FechaVigencia);
