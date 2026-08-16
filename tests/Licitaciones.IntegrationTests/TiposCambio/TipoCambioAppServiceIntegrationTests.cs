using Licitaciones.Application.TiposCambio;
using Licitaciones.Infrastructure.Persistencia;
using Licitaciones.Infrastructure.Repositorios;
using Licitaciones.Infrastructure.Servicios;
using Licitaciones.IntegrationTests.Comunes;

namespace Licitaciones.IntegrationTests.TiposCambio;

/// <summary>
/// Regresión: activar un tipo de cambio nuevo mientras otro está activo
/// violaba el índice único parcial (WHERE "Activo" = true) de PostgreSQL
/// porque EF Core no garantiza que el UPDATE de desactivación se ejecute
/// antes que el de activación dentro del mismo SaveChanges. Solo se
/// reproduce contra PostgreSQL real, no con una lista en memoria.
/// </summary>
[Collection(PostgreSqlCollection.NombreColeccion)]
public class TipoCambioAppServiceIntegrationTests(PostgreSqlFixture fixture)
{
    private static readonly RelojSistema Reloj = new();

    [Fact]
    public async Task ActivarAsync_conOtroTipoDeCambioYaActivo_desactivaElAnteriorSinViolarElIndiceUnico()
    {
        await using var contexto = fixture.CrearContexto();
        var repositorio = new TipoCambioRepository(contexto);
        var unitOfWork = new UnitOfWork(contexto);
        var servicio = new TipoCambioAppService(repositorio, unitOfWork, Reloj);

        var primero = await servicio.CrearAsync(new GuardarTipoCambioRequest(500m, Reloj.UtcAhora));
        await servicio.ActivarAsync(primero.Id);

        var segundo = await servicio.CrearAsync(new GuardarTipoCambioRequest(530m, Reloj.UtcAhora));
        var activado = await servicio.ActivarAsync(segundo.Id);

        Assert.True(activado.Activo);
        var activo = await servicio.ObtenerActivoAsync();
        Assert.Equal(segundo.Id, activo!.Id);
    }
}
