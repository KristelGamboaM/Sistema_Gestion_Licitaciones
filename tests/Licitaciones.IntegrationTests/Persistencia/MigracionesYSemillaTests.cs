using Licitaciones.IntegrationTests.Comunes;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.IntegrationTests.Persistencia;

[Collection(PostgreSqlCollection.NombreColeccion)]
public class MigracionesYSemillaTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Migrar_aplicaTodasLasMigracionesPendientes()
    {
        await using var contexto = fixture.CrearContexto();

        var pendientes = await contexto.Database.GetPendingMigrationsAsync();

        Assert.Empty(pendientes);
    }

    [Fact]
    public async Task DatosSemilla_contieneLosTresNivelesDeAprobacionDelEnunciado()
    {
        await using var contexto = fixture.CrearContexto();

        var niveles = await contexto.NivelesAprobacion.OrderBy(n => n.MontoMinimoCRC).ToListAsync();

        Assert.Equal(3, niveles.Count);
        Assert.Equal("Encargado de área", niveles[0].Aprobador);
        Assert.Equal("Gerencia", niveles[1].Aprobador);
        Assert.Equal("Junta Directiva", niveles[2].Aprobador);
        Assert.True(niveles[2].EsRangoAbierto);
    }

    [Fact]
    public async Task DatosSemilla_contieneUnTipoDeCambioActivo()
    {
        await using var contexto = fixture.CrearContexto();

        var activos = await contexto.TiposCambio.Where(t => t.Activo).ToListAsync();

        Assert.Single(activos);
    }
}
