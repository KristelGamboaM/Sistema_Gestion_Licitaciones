using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;

namespace Licitaciones.FunctionalTests.Comunes;

/// <summary>
/// Levanta PostgreSQL real (Testcontainers), la aplicación Web completa como
/// un proceso Kestrel real (el mismo binario que se ejecutaría en
/// producción, arrancado con <c>dotnet Licitaciones.Web.dll</c>) y un
/// navegador Chromium (Playwright) — spec §12.3: pruebas funcionales de
/// extremo a extremo desde el navegador, no contra un servidor en memoria.
/// </summary>
public sealed class FuncionalWebFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("licitaciones")
        .WithUsername("licitaciones")
        .WithPassword("licitaciones")
        .Build();

    private Process? _procesoWeb;
    private IPlaywright _playwright = null!;
    private IBrowser _navegador = null!;
    private HttpClient _httpClient = null!;

    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();

        var options = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(_contenedor.GetConnectionString())
            .Options;
        await using (var contexto = new LicitacionesDbContext(options))
        {
            await contexto.Database.MigrateAsync();
        }

        var puerto = ObtenerPuertoLibre();
        BaseUrl = $"http://127.0.0.1:{puerto}";

        var rutaDll = Path.Combine(AppContext.BaseDirectory, "Licitaciones.Web.dll");
        _procesoWeb = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{rutaDll}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            EnvironmentVariables =
            {
                ["ASPNETCORE_URLS"] = BaseUrl,
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["ConnectionStrings__LicitacionesDb"] = _contenedor.GetConnectionString(),
            },
        }) ?? throw new InvalidOperationException("No se pudo iniciar el proceso de Licitaciones.Web.");

        _httpClient = new HttpClient();
        await EsperarQueElServidorRespondaAsync();

        _playwright = await Playwright.CreateAsync();
        _navegador = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task<IPage> NuevaPaginaAsync()
    {
        var pagina = await _navegador.NewPageAsync();
        pagina.PageError += (_, error) => Console.WriteLine($"[PageError] {error}");
        pagina.Response += (_, response) =>
        {
            if ((int)response.Status >= 400)
                Console.WriteLine($"[HTTP {response.Status}] {response.Url}");
        };
        return pagina;
    }

    public async Task DisposeAsync()
    {
        if (_navegador is not null)
            await _navegador.CloseAsync();
        _playwright?.Dispose();
        _httpClient?.Dispose();

        if (_procesoWeb is { HasExited: false })
        {
            _procesoWeb.Kill(entireProcessTree: true);
            await _procesoWeb.WaitForExitAsync();
        }
        _procesoWeb?.Dispose();

        await _contenedor.DisposeAsync();
    }

    private async Task EsperarQueElServidorRespondaAsync()
    {
        var limite = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < limite)
        {
            try
            {
                var respuesta = await _httpClient.GetAsync(BaseUrl);
                if (respuesta.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
                // El servidor aún no acepta conexiones; se reintenta.
            }

            await Task.Delay(300);
        }

        throw new TimeoutException($"Licitaciones.Web no respondió en {BaseUrl} dentro del tiempo esperado.");
    }

    private static int ObtenerPuertoLibre()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return puerto;
    }
}

[CollectionDefinition(NombreColeccion)]
public sealed class FuncionalWebCollection : ICollectionFixture<FuncionalWebFixture>
{
    public const string NombreColeccion = "Web funcional (Playwright)";
}
