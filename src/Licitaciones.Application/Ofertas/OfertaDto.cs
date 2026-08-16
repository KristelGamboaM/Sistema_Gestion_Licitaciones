namespace Licitaciones.Application.Ofertas;

public sealed record OfertaDto(
    Guid Id,
    Guid LicitacionId,
    Guid ProveedorId,
    string ProveedorNombre,
    decimal MontoOfertadoCRC,
    DateTimeOffset FechaRegistro,
    DateTimeOffset UpdatedAt);

public sealed record RegistrarOfertaRequest(Guid LicitacionId, Guid ProveedorId, decimal MontoOfertadoCRC);

public sealed record ActualizarOfertaRequest(decimal MontoOfertadoCRC);
