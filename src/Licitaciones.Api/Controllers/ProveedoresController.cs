using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Controllers;

[ApiController]
[Route("api/v1/proveedores")]
[Produces("application/json")]
public sealed class ProveedoresController(IProveedorAppService servicio) : ControllerBase
{
    /// <summary>Lista proveedores con paginación, búsqueda y orden.</summary>
    [HttpGet]
    [ProducesResponseType<PaginaResultado<ProveedorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] bool incluirEliminados = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = await servicio.ListarAsync(
            new ConsultaProveedores(busqueda, incluirEliminados, pagina, tamanoPagina, descendente), cancellationToken);

        return Ok(resultado);
    }

    /// <summary>Consulta un proveedor por identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken cancellationToken) =>
        Ok(await servicio.ObtenerAsync(id, cancellationToken));

    /// <summary>Registra un proveedor nuevo.</summary>
    [HttpPost]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(CrearProveedorRequest solicitud, CancellationToken cancellationToken)
    {
        var creado = await servicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    /// <summary>Edita el nombre de un proveedor existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        Guid id, ActualizarProveedorRequest solicitud, CancellationToken cancellationToken) =>
        Ok(await servicio.ActualizarAsync(id, solicitud, cancellationToken));

    /// <summary>Elimina (borrado lógico) un proveedor.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        await servicio.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
