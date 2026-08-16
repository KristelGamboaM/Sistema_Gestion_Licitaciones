using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class LicitacionFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(300)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El presupuesto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El presupuesto debe ser mayor que cero.")]
    [Display(Name = "Presupuesto estimado (CRC)")]
    public decimal PresupuestoEstimadoCRC { get; set; }

    [Required(ErrorMessage = "La fecha y hora de cierre son obligatorias.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Fecha y hora de cierre (Costa Rica)")]
    public DateTime FechaCierre { get; set; } = DateTime.Now.AddDays(7);
}
