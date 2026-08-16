using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/licitaciones")]
[Produces("application/json")]
public sealed class LicitacionesController(ILicitacionAppService servicio, IOfertaAppService ofertaServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaResultado<LicitacionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] bool descendente = false,
        [FromQuery] ColumnaOrdenLicitacion ordenarPor = ColumnaOrdenLicitacion.FechaCierre,
        CancellationToken cancellationToken = default)
    {
        var resultado = await servicio.ListarAsync(
            new ConsultaLicitaciones(busqueda, Estado: null, IncluirEliminadas: false, pagina, tamanoPagina, descendente, ordenarPor),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(CrearLicitacionRequest solicitud, CancellationToken cancellationToken)
    {
        var creada = await servicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        Guid id, ActualizarLicitacionRequest solicitud, CancellationToken cancellationToken) =>
        Ok(await servicio.ActualizarAsync(id, solicitud, cancellationToken));

    /// <summary>Aplica una transición de estado: <c>Publicar</c> o <c>Cerrar</c>.</summary>
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType<LicitacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(
        Guid id, CambiarEstadoLicitacionRequest solicitud, CancellationToken cancellationToken)
    {
        var actualizada = solicitud.Accion switch
        {
            AccionEstadoLicitacion.Publicar => await servicio.PublicarAsync(id, cancellationToken),
            AccionEstadoLicitacion.Cerrar => await servicio.CerrarAsync(id, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(solicitud)),
        };

        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await servicio.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/mejor-oferta")]
    [ProducesResponseType<MejorOfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerMejorOferta(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerMejorOfertaAsync(id, cancellationToken));

    [HttpGet("{id:guid}/ofertas")]
    [ProducesResponseType<PaginaResultado<OfertaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarOfertas(
        Guid id,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] bool descendente = true,
        [FromQuery] ColumnaOrdenOferta ordenarPor = ColumnaOrdenOferta.FechaRegistro,
        CancellationToken cancellationToken = default) =>
        Ok(await ofertaServicio.ListarAsync(
            new ConsultaOfertas(id, ProveedorId: null, pagina, tamanoPagina, descendente, ordenarPor), cancellationToken));

    [HttpPost("{id:guid}/ofertas")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarOferta(
        Guid id, [FromBody] RegistrarOfertaEnLicitacionRequest solicitud, CancellationToken cancellationToken)
    {
        var creada = await ofertaServicio.RegistrarAsync(
            new RegistrarOfertaRequest(id, solicitud.ProveedorId, solicitud.MontoOfertadoCRC), cancellationToken);
        return CreatedAtAction("Obtener", "Ofertas", new { id = creada.Id }, creada);
    }
}

public sealed record RegistrarOfertaEnLicitacionRequest(Guid ProveedorId, decimal MontoOfertadoCRC);
