namespace Licitaciones.Application.Licitaciones;

public sealed record LicitacionDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Estado,
    bool EsEfectivamenteCerrada,
    DateTimeOffset FechaCierre,
    decimal PresupuestoEstimadoCRC,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CrearLicitacionRequest(
    string Codigo, string Titulo, decimal PresupuestoEstimadoCRC, DateTimeOffset FechaCierre);

public sealed record ActualizarLicitacionRequest(
    string Codigo, string Titulo, decimal PresupuestoEstimadoCRC, DateTimeOffset FechaCierre);

public sealed record MejorOfertaDto(
    bool TieneOfertaValida,
    Guid? ProveedorId,
    string? ProveedorNombre,
    decimal? MontoOfertadoCRC,
    decimal PorcentajeAhorro,
    string Clasificacion,
    string? Aprobador);

public enum AccionEstadoLicitacion
{
    Publicar,
    Cerrar,
}

public sealed record CambiarEstadoLicitacionRequest(AccionEstadoLicitacion Accion);
