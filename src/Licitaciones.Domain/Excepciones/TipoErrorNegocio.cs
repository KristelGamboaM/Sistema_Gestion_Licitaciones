namespace Licitaciones.Domain.Excepciones;

/// <summary>
/// Clasifica una violación de regla de negocio para que las capas
/// superiores (Web, Api) puedan traducirla al código HTTP o mensaje
/// controlado correcto sin volver a interpretar el texto del error.
/// </summary>
public enum TipoErrorNegocio
{
    Validacion,
    Conflicto,
    NoEncontrado,
}
