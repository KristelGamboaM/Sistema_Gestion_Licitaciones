using Licitaciones.Domain.Servicios;

namespace Licitaciones.UnitTests.Servicios;

public class NormalizacionTextoTests
{
    [Theory]
    [InlineData(" lic-2026-001 ", "LIC-2026-001")]
    [InlineData("LIC-2026-001", "LIC-2026-001")]
    [InlineData("Lic-2026-001", "LIC-2026-001")]
    public void NormalizarCodigoLicitacion_ignoraEspaciosLateralesYCaso(string entrada, string esperado)
    {
        Assert.Equal(esperado, NormalizacionTexto.NormalizarCodigoLicitacion(entrada));
    }

    [Theory]
    [InlineData("Empresa Central", "EMPRESA CENTRAL")]
    [InlineData("empresa central", "EMPRESA CENTRAL")]
    [InlineData("  Empresa   Central  ", "EMPRESA CENTRAL")]
    [InlineData("EMPRESA CENTRAL", "EMPRESA CENTRAL")]
    public void NormalizarNombreProveedor_produceLaMismaClaveParaVariantesEquivalentes(string entrada, string esperado)
    {
        Assert.Equal(esperado, NormalizacionTexto.NormalizarNombreProveedor(entrada));
    }

    [Theory]
    [InlineData("Constructora Del Este S.A.", true)]
    [InlineData("Distribuidora (Zona 1, Bloque 2)", true)]
    [InlineData("Proveedor 123", true)]
    [InlineData("Proveedor@Central", false)]
    [InlineData("Proveedor#1", false)]
    [InlineData("Proveedor/Central", false)]
    public void TieneCaracteresPermitidosProveedor_validaElConjuntoDeCaracteres(string nombre, bool esperado)
    {
        Assert.Equal(esperado, NormalizacionTexto.TieneCaracteresPermitidosProveedor(nombre));
    }
}
