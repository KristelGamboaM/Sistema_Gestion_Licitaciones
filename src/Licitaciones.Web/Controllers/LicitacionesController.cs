using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Web.Comun;
using Licitaciones.Web.Models.Licitaciones;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class LicitacionesController(ILicitacionAppService servicio) : Controller
{
    public async Task<IActionResult> Index(
        string? busqueda,
        int pagina = 1,
        ColumnaOrdenLicitacion ordenarPor = ColumnaOrdenLicitacion.FechaCierre,
        bool descendente = false,
        CancellationToken cancellationToken = default)
    {
        const int tamanoPagina = 20;
        var resultado = await servicio.ListarAsync(
            new ConsultaLicitaciones(busqueda, Estado: null, IncluirEliminadas: false, pagina, tamanoPagina, descendente, ordenarPor),
            cancellationToken);

        return View(new LicitacionIndexViewModel
        {
            Licitaciones = resultado.Elementos,
            Busqueda = busqueda,
            Pagina = resultado.Pagina,
            TamanoPagina = resultado.TamanoPagina,
            Total = resultado.Total,
            OrdenarPor = ordenarPor,
            Descendente = descendente,
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var licitacion = await servicio.ObtenerAsync(id, cancellationToken);
            var mejorOferta = await servicio.ObtenerMejorOfertaAsync(id, cancellationToken);
            ViewBag.MejorOferta = mejorOferta;
            return View(licitacion);
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    public IActionResult Create() => View(new LicitacionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LicitacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var solicitud = new CrearLicitacionRequest(
                modelo.Codigo, modelo.Titulo, modelo.PresupuestoEstimadoCRC,
                ZonaHorariaCostaRica.DesdeFormularioLocal(modelo.FechaCierre));

            await servicio.CrearAsync(solicitud, cancellationToken);
            TempData["Mensaje"] = "Licitación creada en estado Borrador.";
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
            var licitacion = await servicio.ObtenerAsync(id, cancellationToken);
            return View(new LicitacionFormViewModel
            {
                Id = licitacion.Id,
                Codigo = licitacion.Codigo,
                Titulo = licitacion.Titulo,
                PresupuestoEstimadoCRC = licitacion.PresupuestoEstimadoCRC,
                FechaCierre = licitacion.FechaCierre.ALocalCostaRica().DateTime,
            });
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LicitacionFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var solicitud = new ActualizarLicitacionRequest(
                modelo.Codigo, modelo.Titulo, modelo.PresupuestoEstimadoCRC,
                ZonaHorariaCostaRica.DesdeFormularioLocal(modelo.FechaCierre));

            await servicio.ActualizarAsync(id, solicitud, cancellationToken);
            TempData["Mensaje"] = "Licitación actualizada correctamente.";
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
    public async Task<IActionResult> Publicar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await servicio.PublicarAsync(id, cancellationToken);
            TempData["Mensaje"] = "Licitación publicada.";
        }
        catch (ReglaNegocioException excepcion)
        {
            TempData["Error"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await servicio.CerrarAsync(id, cancellationToken);
            TempData["Mensaje"] = "Licitación cerrada.";
        }
        catch (ReglaNegocioException excepcion)
        {
            TempData["Error"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var licitacion = await servicio.ObtenerAsync(id, cancellationToken);
            return View(licitacion);
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
            TempData["Mensaje"] = "Licitación eliminada correctamente.";
        }
        catch (ReglaNegocioException excepcion)
        {
            TempData["Error"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
