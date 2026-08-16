using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Web.Models.NivelesAprobacion;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class NivelesAprobacionController(INivelAprobacionAppService servicio) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await servicio.ListarAsync(cancellationToken));

    public IActionResult Create() => View(new NivelAprobacionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NivelAprobacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.CrearAsync(
                new GuardarNivelAprobacionRequest(modelo.MontoMinimoCRC, modelo.MontoMaximoCRC, modelo.Aprobador),
                cancellationToken);
            TempData["Mensaje"] = "Nivel de aprobación creado correctamente.";
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
            var nivel = await servicio.ObtenerAsync(id, cancellationToken);
            return View(new NivelAprobacionFormViewModel
            {
                Id = nivel.Id,
                MontoMinimoCRC = nivel.MontoMinimoCRC,
                MontoMaximoCRC = nivel.MontoMaximoCRC,
                Aprobador = nivel.Aprobador,
            });
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NivelAprobacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.ActualizarAsync(
                id, new GuardarNivelAprobacionRequest(modelo.MontoMinimoCRC, modelo.MontoMaximoCRC, modelo.Aprobador),
                cancellationToken);
            TempData["Mensaje"] = "Nivel de aprobación actualizado correctamente.";
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
            return View(await servicio.ObtenerAsync(id, cancellationToken));
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
        await servicio.EliminarAsync(id, cancellationToken);
        TempData["Mensaje"] = "Nivel de aprobación eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
