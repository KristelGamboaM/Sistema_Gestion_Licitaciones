namespace Licitaciones.Application.NivelesAprobacion;

public sealed record NivelAprobacionDto(
    Guid Id,
    decimal MontoMinimoCRC,
    decimal? MontoMaximoCRC,
    string Aprobador,
    bool EsRangoAbierto,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record GuardarNivelAprobacionRequest(decimal MontoMinimoCRC, decimal? MontoMaximoCRC, string Aprobador);
