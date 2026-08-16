using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.UnitTests.Comunes;

namespace Licitaciones.UnitTests.Entidades;

public class ProveedorTests
{
    private static readonly RelojFalso Reloj = RelojFalso.EnUtc(2026, 1, 10);

    [Fact]
    public void Crear_conNombreValido_normalizaYAsignaId()
    {
        var proveedor = Proveedor.Crear("Empresa Central", Reloj);

        Assert.NotEqual(Guid.Empty, proveedor.Id);
        Assert.Equal("Empresa Central", proveedor.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedor.NombreNormalizado);
        Assert.False(proveedor.EstaEliminado);
    }

    [Theory]
    [InlineData("Empresa Central")]
    [InlineData("empresa central")]
    [InlineData("EMPRESA CENTRAL")]
    [InlineData("  Empresa   Central  ")]
    public void Crear_variantesEquivalentes_producenElMismoNombreNormalizado(string nombre)
    {
        var proveedor = Proveedor.Crear(nombre, Reloj);

        Assert.Equal("EMPRESA CENTRAL", proveedor.NombreNormalizado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_conNombreVacio_lanzaReglaNegocio(string nombre)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => Proveedor.Crear(nombre, Reloj));

        Assert.Equal("proveedor.nombre.requerido", excepcion.Codigo);
    }

    [Theory]
    [InlineData("Empresa @Central")]
    [InlineData("Empresa#1")]
    [InlineData("Empresa/Central")]
    public void Crear_conCaracteresNoPermitidos_lanzaReglaNegocio(string nombre)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => Proveedor.Crear(nombre, Reloj));

        Assert.Equal("proveedor.nombre.caracteres_invalidos", excepcion.Codigo);
    }

    [Fact]
    public void Crear_conCaracteresPermitidos_noLanza()
    {
        var proveedor = Proveedor.Crear("Constructora Del Este S.A. (Zona 1, Bloque 2)", Reloj);

        Assert.Equal("Constructora Del Este S.A. (Zona 1, Bloque 2)", proveedor.Nombre);
    }

    [Fact]
    public void Eliminar_marcaDeletedAtYEsIdempotente()
    {
        var proveedor = Proveedor.Crear("Proveedor Uno", Reloj);

        proveedor.Eliminar(Reloj);
        var primeraEliminacion = proveedor.DeletedAt;
        proveedor.Eliminar(Reloj);

        Assert.True(proveedor.EstaEliminado);
        Assert.Equal(primeraEliminacion, proveedor.DeletedAt);
    }

    [Fact]
    public void Actualizar_sobreProveedorEliminado_lanzaReglaNegocio()
    {
        var proveedor = Proveedor.Crear("Proveedor Uno", Reloj);
        proveedor.Eliminar(Reloj);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => proveedor.Actualizar("Proveedor Dos", Reloj));

        Assert.Equal("proveedor.eliminado", excepcion.Codigo);
    }
}
