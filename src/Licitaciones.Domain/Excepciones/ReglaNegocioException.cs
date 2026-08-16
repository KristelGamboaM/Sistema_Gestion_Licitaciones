namespace Licitaciones.Domain.Excepciones;

/// <summary>
/// Señala la violación de una regla de negocio del dominio de licitaciones.
/// El <see cref="Codigo"/> es estable y apto para mapear a mensajes de UI
/// o a <c>ProblemDetails</c> sin exponer detalles técnicos internos.
/// </summary>
public sealed class ReglaNegocioException : Exception
{
    public string Codigo { get; }
    public TipoErrorNegocio Tipo { get; }

    public ReglaNegocioException(string codigo, string mensaje, TipoErrorNegocio tipo = TipoErrorNegocio.Validacion)
        : base(mensaje)
    {
        Codigo = codigo;
        Tipo = tipo;
    }
}
