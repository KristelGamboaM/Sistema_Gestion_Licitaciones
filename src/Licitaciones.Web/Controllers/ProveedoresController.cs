using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Web.Models.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class ProveedoresController(IProveedorAppService servicio) : Controller
{
    public async Task<IActionResult> Index(string? busqueda, int pagina = 1, CancellationToken cancellationToken = default)
    {
        const int tamanoPagina = 20;
        var resultado = await servicio.ListarAsync(
            new ConsultaProveedores(busqueda, IncluirEliminados: false, pagina, tamanoPagina), cancellationToken);

        return View(new ProveedorIndexViewModel
        {
            Proveedores = resultado.Elementos,
            Busqueda = busqueda,
            Pagina = resultado.Pagina,
            TamanoPagina = resultado.TamanoPagina,
            Total = resultado.Total,
        });
    }

    public IActionResult Create() => View(new ProveedorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.CrearAsync(new CrearProveedorRequest(modelo.Nombre), cancellationToken);
            TempData["Mensaje"] = "Proveedor registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException excepcion)
        {
            ModelState.AddModelError(string.Empty, excepcion.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var proveedor = await servicio.ObtenerAsync(id, cancellationToken);
            return View(new ProveedorFormViewModel { Id = proveedor.Id, Nombre = proveedor.Nombre });
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProveedorFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.ActualizarAsync(id, new ActualizarProveedorRequest(modelo.Nombre), cancellationToken);
            TempData["Mensaje"] = "Proveedor actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException excepcion)
        {
            ModelState.AddModelError(string.Empty, excepcion.Message);
            return View(modelo);
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var proveedor = await servicio.ObtenerAsync(id, cancellationToken);
            return View(proveedor);
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await servicio.EliminarAsync(id, cancellationToken);
            TempData["Mensaje"] = "Proveedor eliminado correctamente.";
        }
        catch (ReglaNegocioException excepcion)
        {
            TempData["Error"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
