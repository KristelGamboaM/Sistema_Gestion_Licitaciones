namespace Licitaciones.Domain.Excepciones;

/// <summary>
/// Se lanza cuando se solicita una entidad por identificador y no existe
/// (o fue eliminada lógicamente). Las capas Web/Api la traducen a 404.
/// </summary>
public sealed class EntidadNoEncontradaException : Exception
{
    public EntidadNoEncontradaException(string entidad, Guid id)
        : base($"{entidad} con id '{id}' no fue encontrado.")
    {
    }
}
