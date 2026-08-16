using Licitaciones.Application.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/niveles-aprobacion")]
[Produces("application/json")]
public sealed class NivelesAprobacionController(INivelAprobacionAppService servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NivelAprobacionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken)
    {
        var creado = await servicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<NivelAprobacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        Guid id, GuardarNivelAprobacionRequest solicitud, CancellationToken cancellationToken) =>
        Ok(await servicio.ActualizarAsync(id, solicitud, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await servicio.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
