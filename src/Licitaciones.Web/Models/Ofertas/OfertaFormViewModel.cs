using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Ofertas;

public sealed class OfertaFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid LicitacionId { get; set; }

    public string? LicitacionCodigo { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
    [Display(Name = "Proveedor")]
    public Guid ProveedorId { get; set; }

    public IEnumerable<ProveedorOpcion> ProveedoresDisponibles { get; set; } = [];

    [Required(ErrorMessage = "El monto ofertado es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto ofertado debe ser mayor que cero.")]
    [Display(Name = "Monto ofertado (CRC)")]
    public decimal MontoOfertadoCRC { get; set; }
}

/// <summary>
/// Opción de proveedor para el &lt;select&gt; del formulario. Debe ser un
/// tipo con propiedades reales (no una tupla con nombres): <c>SelectList</c>
/// resuelve "Id"/"Nombre" por reflexión, y una tupla solo expone
/// <c>Item1</c>/<c>Item2</c> en tiempo de ejecución — los nombres de una
/// tupla son azúcar sintáctica del compilador, no miembros reflectables.
/// </summary>
public sealed record ProveedorOpcion(Guid Id, string Nombre);
