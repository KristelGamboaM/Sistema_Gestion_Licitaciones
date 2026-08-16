using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Web.Comun;
using Licitaciones.Web.Models.TiposCambio;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class TiposCambioController(ITipoCambioAppService servicio) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await servicio.ListarAsync(cancellationToken));

    public IActionResult Create() => View(new TipoCambioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TipoCambioFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.CrearAsync(
                new GuardarTipoCambioRequest(modelo.CRCporUSD, ZonaHorariaCostaRica.DesdeFormularioLocal(modelo.FechaVigencia)),
                cancellationToken);
            TempData["Mensaje"] = "Tipo de cambio registrado. Actívelo para que la conversión lo utilice.";
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
            var tipoCambio = await servicio.ObtenerAsync(id, cancellationToken);
            return View(new TipoCambioFormViewModel
            {
                Id = tipoCambio.Id,
                CRCporUSD = tipoCambio.CRCporUSD,
                FechaVigencia = tipoCambio.FechaVigencia.ALocalCostaRica().DateTime,
            });
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TipoCambioFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.ActualizarAsync(
                id, new GuardarTipoCambioRequest(modelo.CRCporUSD, ZonaHorariaCostaRica.DesdeFormularioLocal(modelo.FechaVigencia)),
                cancellationToken);
            TempData["Mensaje"] = "Tipo de cambio actualizado correctamente.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken)
    {
        await servicio.ActivarAsync(id, cancellationToken);
        TempData["Mensaje"] = "Tipo de cambio activado.";
        return RedirectToAction(nameof(Index));
    }
}
