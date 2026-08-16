using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.TiposCambio;

public sealed class TipoCambioFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "La tasa es obligatoria.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa debe ser mayor que cero.")]
    [Display(Name = "Colones por dólar (CRC por USD)")]
    public decimal CRCporUSD { get; set; }

    [Required(ErrorMessage = "La fecha de vigencia es obligatoria.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Fecha de vigencia")]
    public DateTime FechaVigencia { get; set; } = DateTime.Today;
}
