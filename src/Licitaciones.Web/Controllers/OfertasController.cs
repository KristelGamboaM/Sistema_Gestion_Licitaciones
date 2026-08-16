using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

public sealed class OfertasController(
    IOfertaAppService servicio, ILicitacionAppService licitacionServicio, IProveedorAppService proveedorServicio)
    : Controller
{
    public async Task<IActionResult> Index(
        Guid? licitacionId,
        int pagina = 1,
        ColumnaOrdenOferta ordenarPor = ColumnaOrdenOferta.FechaRegistro,
        bool descendente = true,
        CancellationToken cancellationToken = default)
    {
        const int tamanoPagina = 20;
        var resultado = await servicio.ListarAsync(
            new ConsultaOfertas(licitacionId, ProveedorId: null, pagina, tamanoPagina, descendente, ordenarPor),
            cancellationToken);

        string? codigo = null;
        if (licitacionId is not null)
        {
            try
            {
                codigo = (await licitacionServicio.ObtenerAsync(licitacionId.Value, cancellationToken)).Codigo;
            }
            catch (EntidadNoEncontradaException)
            {
                return NotFound();
            }
        }

        return View(new OfertaIndexViewModel
        {
            Ofertas = resultado.Elementos,
            LicitacionId = licitacionId,
            LicitacionCodigo = codigo,
            Pagina = resultado.Pagina,
            TamanoPagina = resultado.TamanoPagina,
            Total = resultado.Total,
            OrdenarPor = ordenarPor,
            Descendente = descendente,
        });
    }

    public async Task<IActionResult> Create(Guid licitacionId, CancellationToken cancellationToken)
    {
        try
        {
            var licitacion = await licitacionServicio.ObtenerAsync(licitacionId, cancellationToken);
            var modelo = new OfertaFormViewModel
            {
                LicitacionId = licitacionId,
                LicitacionCodigo = licitacion.Codigo,
                ProveedoresDisponibles = await ObtenerProveedoresAsync(cancellationToken),
            };
            return View(modelo);
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OfertaFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            modelo.ProveedoresDisponibles = await ObtenerProveedoresAsync(cancellationToken);
            return View(modelo);
        }

        try
        {
            await servicio.RegistrarAsync(
                new RegistrarOfertaRequest(modelo.LicitacionId, modelo.ProveedorId, modelo.MontoOfertadoCRC),
                cancellationToken);
            TempData["Mensaje"] = "Oferta registrada correctamente.";
            return RedirectToAction(nameof(Index), new { licitacionId = modelo.LicitacionId });
        }
        catch (ReglaNegocioException excepcion)
        {
            ModelState.AddModelError(string.Empty, excepcion.Message);
            modelo.ProveedoresDisponibles = await ObtenerProveedoresAsync(cancellationToken);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var oferta = await servicio.ObtenerAsync(id, cancellationToken);
            var licitacion = await licitacionServicio.ObtenerAsync(oferta.LicitacionId, cancellationToken);
            return View(new OfertaFormViewModel
            {
                Id = oferta.Id,
                LicitacionId = oferta.LicitacionId,
                LicitacionCodigo = licitacion.Codigo,
                ProveedorId = oferta.ProveedorId,
                MontoOfertadoCRC = oferta.MontoOfertadoCRC,
                ProveedoresDisponibles = [new ProveedorOpcion(oferta.ProveedorId, oferta.ProveedorNombre)],
            });
        }
        catch (EntidadNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OfertaFormViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await servicio.ActualizarAsync(id, new ActualizarOfertaRequest(modelo.MontoOfertadoCRC), cancellationToken);
            TempData["Mensaje"] = "Oferta actualizada correctamente.";
            return RedirectToAction(nameof(Index), new { licitacionId = modelo.LicitacionId });
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
    public async Task<IActionResult> DeleteConfirmed(Guid id, Guid licitacionId, CancellationToken cancellationToken)
    {
        try
        {
            await servicio.EliminarAsync(id, cancellationToken);
            TempData["Mensaje"] = "Oferta eliminada correctamente.";
        }
        catch (ReglaNegocioException excepcion)
        {
            TempData["Error"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Index), new { licitacionId });
    }

    private async Task<IEnumerable<ProveedorOpcion>> ObtenerProveedoresAsync(CancellationToken cancellationToken)
    {
        var pagina = await proveedorServicio.ListarAsync(
            new ConsultaProveedores(TamanoPagina: 500), cancellationToken);
        return pagina.Elementos.Select(p => new ProveedorOpcion(p.Id, p.Nombre));
    }
}
