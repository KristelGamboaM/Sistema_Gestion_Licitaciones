using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Comunes;

/// <summary>
/// Levanta un contenedor PostgreSQL 16 real (Testcontainers) una única vez
/// por colección de pruebas, aplica las migraciones y expone contextos
/// nuevos por prueba — spec §12.2: "Ejecución contra PostgreSQL real en
/// contenedor". No se usa SQLite ni una base en memoria.
/// </summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("licitaciones")
        .WithUsername("licitaciones")
        .WithPassword("licitaciones")
        .Build();

    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();

        await using var contexto = CrearContexto();
        await contexto.Database.MigrateAsync();
    }

    public LicitacionesDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(_contenedor.GetConnectionString())
            .Options;

        return new LicitacionesDbContext(options);
    }

    public Task DisposeAsync() => _contenedor.DisposeAsync().AsTask();
}

[CollectionDefinition(NombreColeccion)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string NombreColeccion = "PostgreSQL real";
}
