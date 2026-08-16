using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Infrastructure.Repositorios;
using Licitaciones.Infrastructure.Servicios;
using Licitaciones.IntegrationTests.Comunes;

namespace Licitaciones.IntegrationTests.Persistencia;

/// <summary>Ejercita los repositorios reales (no el DbContext directamente) contra PostgreSQL.</summary>
[Collection(PostgreSqlCollection.NombreColeccion)]
public class RepositoriosTests(PostgreSqlFixture fixture)
{
    private static readonly RelojSistema Reloj = new();

    [Fact]
    public async Task ProveedorRepository_agregarObtenerYListarConBusqueda_funcionaContraPostgres()
    {
        await using var contexto = fixture.CrearContexto();
        var repositorio = new ProveedorRepository(contexto);
        var nombre = $"Proveedor Repo {Guid.NewGuid():N}"[..40];
        var proveedor = Proveedor.Crear(nombre, Reloj);

        repositorio.Agregar(proveedor);
        await contexto.SaveChangesAsync();

        var obtenido = await repositorio.ObtenerPorIdAsync(proveedor.Id);
        Assert.NotNull(obtenido);
        Assert.Equal(nombre, obtenido!.Nombre);

        var pagina = await repositorio.ListarAsync(new ConsultaProveedores(Busqueda: nombre[..20]));
        Assert.Contains(pagina.Elementos, p => p.Id == proveedor.Id);

        var existe = await repositorio.ExisteNombreNormalizadoAsync(proveedor.NombreNormalizado);
        Assert.True(existe);
    }

    [Fact]
    public async Task LicitacionRepository_agregarObtenerYListarPorEstado_funcionaContraPostgres()
    {
        await using var contexto = fixture.CrearContexto();
        var repositorio = new LicitacionRepository(contexto);
        var codigo = $"LIC-REPO-{Guid.NewGuid():N}"[..20];
        var licitacion = Licitacion.Crear(codigo, "Título de prueba", 500_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        licitacion.Publicar(Reloj);

        repositorio.Agregar(licitacion);
        await contexto.SaveChangesAsync();

        var obtenida = await repositorio.ObtenerPorIdAsync(licitacion.Id);
        Assert.NotNull(obtenida);
        Assert.Equal(EstadoLicitacion.Publicada, obtenida!.Estado);

        var pagina = await repositorio.ListarAsync(new ConsultaLicitaciones(Estado: EstadoLicitacion.Publicada));
        Assert.Contains(pagina.Elementos, l => l.Id == licitacion.Id);

        var existeCodigo = await repositorio.ExisteCodigoNormalizadoAsync(licitacion.CodigoNormalizado);
        Assert.True(existeCodigo);
    }

    [Fact]
    public async Task OfertaRepository_agregarListarYObtenerMontoMayor_funcionaContraPostgres()
    {
        await using var contexto = fixture.CrearContexto();
        var proveedorRepositorio = new ProveedorRepository(contexto);
        var licitacionRepositorio = new LicitacionRepository(contexto);
        var ofertaRepositorio = new OfertaRepository(contexto);

        var proveedor = Proveedor.Crear($"Proveedor Oferta Repo {Guid.NewGuid():N}"[..40], Reloj);
        var licitacion = Licitacion.Crear(
            $"LIC-OF-REPO-{Guid.NewGuid():N}"[..20], "Título", 1_000_000m, Reloj.UtcAhora.AddDays(5), Reloj);
        proveedorRepositorio.Agregar(proveedor);
        licitacionRepositorio.Agregar(licitacion);
        await contexto.SaveChangesAsync();

        var oferta = Oferta.Registrar(licitacion.Id, proveedor.Id, 750_000m, Reloj);
        ofertaRepositorio.Agregar(oferta);
        await contexto.SaveChangesAsync();

        var existeOferta = await ofertaRepositorio.ExisteOfertaDeProveedorAsync(licitacion.Id, proveedor.Id);
        Assert.True(existeOferta);

        var ofertasDeLicitacion = await ofertaRepositorio.ListarPorLicitacionAsync(licitacion.Id);
        Assert.Single(ofertasDeLicitacion);

        var montoMayor = await ofertaRepositorio.ObtenerMontoMayorAsync(licitacion.Id);
        Assert.Equal(750_000m, montoMayor);

        var pagina = await ofertaRepositorio.ListarAsync(new ConsultaOfertas(LicitacionId: licitacion.Id));
        Assert.Single(pagina.Elementos);

        var obtenida = await ofertaRepositorio.ObtenerPorIdAsync(oferta.Id);
        Assert.NotNull(obtenida);

        ofertaRepositorio.Eliminar(obtenida!);
        await contexto.SaveChangesAsync();

        var trasEliminar = await ofertaRepositorio.ObtenerPorIdAsync(oferta.Id);
        Assert.Null(trasEliminar);
    }

    [Fact]
    public async Task NivelAprobacionRepository_listarIncluyeLaSemillaYPermiteAgregarYEliminar()
    {
        await using var contexto = fixture.CrearContexto();
        var repositorio = new NivelAprobacionRepository(contexto);

        var niveles = await repositorio.ListarTodosAsync();
        Assert.True(niveles.Count >= 3);

        var nuevo = NivelAprobacion.Crear(50_000_000m, 60_000_000m, $"Aprobador {Guid.NewGuid():N}"[..20], Reloj);
        repositorio.Agregar(nuevo);
        await contexto.SaveChangesAsync();

        var obtenido = await repositorio.ObtenerPorIdAsync(nuevo.Id);
        Assert.NotNull(obtenido);

        repositorio.Eliminar(obtenido!);
        await contexto.SaveChangesAsync();

        Assert.Null(await repositorio.ObtenerPorIdAsync(nuevo.Id));
    }
}
