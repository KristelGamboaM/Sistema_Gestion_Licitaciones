using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Servicios;

/// <summary>
/// Reglas de normalización de texto usadas para validar unicidad de código
/// de licitación y nombre de proveedor (spec §8.3/§8.4), independientes de
/// cualquier motor de persistencia.
/// </summary>
public static partial class NormalizacionTexto
{
    [GeneratedRegex(@"^[\p{L}\p{N} .,()]+$")]
    private static partial Regex CaracteresPermitidosProveedor();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosRepetidos();

    /// <summary>Clave de comparación de código de licitación: trim + mayúsculas.</summary>
    public static string NormalizarCodigoLicitacion(string codigo) =>
        codigo.Trim().ToUpperInvariant();

    /// <summary>
    /// Clave de comparación de nombre de proveedor: trim, colapso de
    /// espacios repetidos, normalización Unicode NFKC y mayúsculas.
    /// </summary>
    public static string NormalizarNombreProveedor(string nombre)
    {
        var sinEspaciosLaterales = nombre.Trim().Normalize(NormalizationForm.FormKC);
        var sinEspaciosRepetidos = EspaciosRepetidos().Replace(sinEspaciosLaterales, " ");
        return sinEspaciosRepetidos.ToUpperInvariant();
    }

    /// <summary>Letras, números, espacios, punto, coma y paréntesis únicamente.</summary>
    public static bool TieneCaracteresPermitidosProveedor(string nombre) =>
        CaracteresPermitidosProveedor().IsMatch(nombre);
}
