namespace Licitaciones.Domain.Abstracciones;

/// <summary>
/// Abstrae la hora actual del sistema para que las reglas de vencimiento y
/// cierre de licitaciones sean deterministas y verificables en pruebas.
/// </summary>
public interface IReloj
{
    /// <summary>Instante actual en UTC. Toda comparación de negocio se hace en UTC.</summary>
    DateTimeOffset UtcAhora { get; }
}
