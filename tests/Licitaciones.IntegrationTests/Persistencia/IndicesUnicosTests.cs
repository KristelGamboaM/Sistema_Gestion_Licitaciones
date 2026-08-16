using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Infrastructure.Persistencia;
using Licitaciones.Infrastructure.Servicios;
using Licitaciones.IntegrationTests.Comunes;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.IntegrationTests.Persistencia;

[Collection(PostgreSqlCollection.NombreColeccion)]
public class IndicesUnicosTests(PostgreSqlFixture fixture)
{
    private static readonly RelojSistema Reloj = new();

    [Fact]
    public async Task Proveedores_nombreNormalizadoDuplicado_violaIndiceUnico()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var nombre = $"Constructora Única {sufijo}";

        await using (var contexto = fixture.CrearContexto())
        {
            contexto.Proveedores.Add(Proveedor.Crear(nombre, Reloj));
            await contexto.SaveChangesAsync();
        }

        await using var segundoContexto = fixture.CrearContexto();
        segundoContexto.Proveedores.Add(Proveedor.Crear(nombre.ToUpperInvariant(), Reloj));

        await Assert.ThrowsAsync<DbUpdateException>(() => segundoContexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Licitaciones_codigoNormalizadoDuplicado_violaIndiceUnico()
    {
        var codigo = $"LIC-{Guid.NewGuid():N}"[..20];

        await using (var contexto = fixture.CrearContexto())
        {
            contexto.Licitaciones.Add(
                Licitacion.Crear(codigo, "Título", 100_000m, Reloj.UtcAhora.AddDays(5), Reloj));
            await contexto.SaveChangesAsync();
        }

        await using var segundoContexto = fixture.CrearContexto();
        segundoContexto.Licitaciones.Add(
            Licitacion.Crear(codigo.ToLowerInvariant(), "Otro título", 200_000m, Reloj.UtcAhora.AddDays(10), Reloj));

        await Assert.ThrowsAsync<DbUpdateException>(() => segundoContexto.SaveChangesAsync());
    }

    [Fact]
    public async Task Ofertas_mismoProveedorMismaLicitacion_violaIndiceUnicoCompuesto()
    {
        await using var contextoSemilla = fixture.CrearContexto();
        var licitacion = Licitacion.Crear(
            $"LIC-{Guid.NewGuid():N}"[..20], "Título", 1_000_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        var proveedor = Proveedor.Crear($"Proveedor {Guid.NewGuid():N}"[..30], Reloj);
        contextoSemilla.Licitaciones.Add(licitacion);
        contextoSemilla.Proveedores.Add(proveedor);
        await contextoSemilla.SaveChangesAsync();

        await using (var contexto = fixture.CrearContexto())
        {
            contexto.Ofertas.Add(Oferta.Registrar(licitacion.Id, proveedor.Id, 500_000m, Reloj));
            await contexto.SaveChangesAsync();
        }

        await using var segundoContexto = fixture.CrearContexto();
        segundoContexto.Ofertas.Add(Oferta.Registrar(licitacion.Id, proveedor.Id, 600_000m, Reloj));

        await Assert.ThrowsAsync<DbUpdateException>(() => segundoContexto.SaveChangesAsync());
    }

    [Fact]
    public async Task UnitOfWork_traduceViolacionDeIndiceUnico_aReglaNegocioDeConflicto()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var nombre = $"Proveedor Traducido {sufijo}";

        await using var contextoUno = fixture.CrearContexto();
        contextoUno.Proveedores.Add(Proveedor.Crear(nombre, Reloj));
        await contextoUno.SaveChangesAsync();

        await using var contextoDos = fixture.CrearContexto();
        contextoDos.Proveedores.Add(Proveedor.Crear(nombre, Reloj));
        var unitOfWork = new UnitOfWork(contextoDos);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => unitOfWork.GuardarCambiosAsync());

        Assert.Equal(TipoErrorNegocio.Conflicto, excepcion.Tipo);
        Assert.Equal("integridad.duplicado", excepcion.Codigo);
    }
}
