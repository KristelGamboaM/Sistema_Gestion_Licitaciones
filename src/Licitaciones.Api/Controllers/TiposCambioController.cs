using Licitaciones.Application.TiposCambio;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/tipos-cambio")]
[Produces("application/json")]
public sealed class TiposCambioController(ITipoCambioAppService servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TipoCambioDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(cancellationToken));

    [HttpGet("activo")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerActivo(CancellationToken cancellationToken)
    {
        var activo = await servicio.ObtenerActivoAsync(cancellationToken);
        return activo is null ? NotFound() : Ok(activo);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken)
    {
        var creado = await servicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        Guid id, GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken) =>
        Ok(await servicio.ActualizarAsync(id, solicitud, cancellationToken));

    [HttpPatch("{id:guid}/activar")]
    [ProducesResponseType<TipoCambioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ActivarAsync(id, cancellationToken));

    /// <summary>Convierte un monto en CRC a USD usando el tipo de cambio activo.</summary>
    [HttpGet("convertir")]
    [ProducesResponseType<ConversionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Convertir([FromQuery] decimal montoCRC, CancellationToken cancellationToken) =>
        Ok(await servicio.ConvertirCrcAUsdAsync(montoCRC, cancellationToken));
}
