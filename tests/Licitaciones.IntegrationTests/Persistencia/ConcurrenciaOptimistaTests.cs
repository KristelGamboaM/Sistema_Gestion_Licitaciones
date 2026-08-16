using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Infrastructure.Persistencia;
using Licitaciones.Infrastructure.Servicios;
using Licitaciones.IntegrationTests.Comunes;

namespace Licitaciones.IntegrationTests.Persistencia;

/// <summary>US-17: la columna de sistema xmin de PostgreSQL detecta ediciones concurrentes.</summary>
[Collection(PostgreSqlCollection.NombreColeccion)]
public class ConcurrenciaOptimistaTests(PostgreSqlFixture fixture)
{
    private static readonly RelojSistema Reloj = new();

    [Fact]
    public async Task EditarElMismoProveedorDesdeDosContextos_elSegundoGuardadoLanzaConflicto()
    {
        var proveedorId = await CrearProveedorAsync($"Proveedor Concurrencia {Guid.NewGuid():N}"[..40]);

        await using var contextoA = fixture.CrearContexto();
        await using var contextoB = fixture.CrearContexto();

        var proveedorA = (await contextoA.Proveedores.FindAsync(proveedorId))!;
        var proveedorB = (await contextoB.Proveedores.FindAsync(proveedorId))!;

        proveedorA.Actualizar("Nombre Actualizado Primero", Reloj);
        await new UnitOfWork(contextoA).GuardarCambiosAsync();

        proveedorB.Actualizar("Nombre Actualizado Segundo", Reloj);
        var unitOfWorkB = new UnitOfWork(contextoB);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => unitOfWorkB.GuardarCambiosAsync());

        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
        Assert.Equal("concurrencia.conflicto", excepcion.Codigo);
    }

    private async Task<Guid> CrearProveedorAsync(string nombre)
    {
        await using var contexto = fixture.CrearContexto();
        var proveedor = Proveedor.Crear(nombre, Reloj);
        contexto.Proveedores.Add(proveedor);
        await contexto.SaveChangesAsync();
        return proveedor.Id;
    }
}
