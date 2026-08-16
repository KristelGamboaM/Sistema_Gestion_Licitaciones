namespace Licitaciones.Domain.Comun;

/// <summary>
/// Base para toda entidad de dominio: garantiza un identificador generado
/// automáticamente y no editable por la persona usuaria.
/// </summary>
public abstract class EntidadBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
