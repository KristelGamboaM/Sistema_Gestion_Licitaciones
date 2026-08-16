using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/ofertas")]
[Produces("application/json")]
public sealed class OfertasController(IOfertaAppService servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaResultado<OfertaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? licitacionId,
        [FromQuery] Guid? proveedorId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] bool descendente = true,
        [FromQuery] ColumnaOrdenOferta ordenarPor = ColumnaOrdenOferta.FechaRegistro,
        CancellationToken cancellationToken = default) =>
        Ok(await servicio.ListarAsync(
            new ConsultaOfertas(licitacionId, proveedorId, pagina, tamanoPagina, descendente, ordenarPor), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registrar(RegistrarOfertaRequest solicitud, CancellationToken cancellationToken)
    {
        var creada = await servicio.RegistrarAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<OfertaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Actualizar(Guid id, ActualizarOfertaRequest solicitud, CancellationToken cancellationToken) =>
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
