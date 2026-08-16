using Licitaciones.Domain.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.ManejoErrores;

/// <summary>
/// Traduce las excepciones de negocio del dominio a <c>ProblemDetails</c>
/// con el código HTTP correcto, sin exponer stack traces, rutas internas
/// ni mensajes técnicos (spec §10.2). Cualquier otra excepción no prevista
/// se deja pasar al manejador de errores por defecto de ASP.NET Core.
/// </summary>
public sealed class ExcepcionesDeNegocioHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo, codigo) = exception switch
        {
            EntidadNoEncontradaException => (StatusCodes.Status404NotFound, "Recurso no encontrado", "recurso.no_encontrado"),
            ReglaNegocioException regla => (MapearEstado(regla.Tipo), "Regla de negocio incumplida", regla.Codigo),
            _ => (0, string.Empty, string.Empty),
        };

        if (status == 0)
            return false;

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = exception.Message,
                Extensions =
                {
                    ["codigo"] = codigo,
                    ["correlacionId"] = httpContext.TraceIdentifier,
                },
            },
        });
    }

    private static int MapearEstado(TipoErrorNegocio tipo) => tipo switch
    {
        TipoErrorNegocio.Conflicto => StatusCodes.Status409Conflict,
        TipoErrorNegocio.NoEncontrado => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status422UnprocessableEntity,
    };
}
