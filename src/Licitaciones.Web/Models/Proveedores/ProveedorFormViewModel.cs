using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Proveedores;

public sealed class ProveedorFormViewModel
{
    public Guid? Id { get; set; }

    // Nota: la regla canónica (spec §8.4) usa \p{L}\p{N} (Unicode completo) y
    // se aplica en el dominio (NormalizacionTexto), que es la autoridad real.
    // Aquí, para la validación del lado del cliente, se usa un patrón
    // equivalente para letras latinas con tilde: jQuery Validate traduce esta
    // expresión a un RegExp de JavaScript SIN el flag "u", donde \p{L} no es
    // una clase Unicode válida y rechazaría cualquier nombre con letras.
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    [RegularExpression(@"^[a-zA-ZÀ-ÖØ-öø-ÿ0-9 .,()]+$", ErrorMessage = "Solo se permiten letras, números, espacios, punto, coma y paréntesis.")]
    [Display(Name = "Nombre del proveedor")]
    public string Nombre { get; set; } = string.Empty;
}
