using System.Text.RegularExpressions;
using Licitaciones.FunctionalTests.Comunes;
using Microsoft.Playwright;

namespace Licitaciones.FunctionalTests;

/// <summary>US-13/US-14: landing page, navegación y modo claro/oscuro.</summary>
[Collection(FuncionalWebCollection.NombreColeccion)]
public class LandingYNavegacionTests(FuncionalWebFixture fixture)
{
    [Fact]
    public async Task LandingPage_muestraElFlujoYEnlazaLosModulos()
    {
        var pagina = await fixture.NuevaPaginaAsync();
        await pagina.GotoAsync(fixture.BaseUrl);

        await Assertions.Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Sistema de Gestión de Licitaciones" }))
            .ToBeVisibleAsync();

        foreach (var modulo in new[] { "Licitaciones", "Proveedores", "Ofertas", "Niveles de aprobación", "Tipo de cambio" })
        {
            await Assertions.Expect(pagina.GetByRole(AriaRole.Link, new() { Name = modulo, Exact = true }).First)
                .ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task NavegacionEntreModulos_llegaAlListadoDeProveedores()
    {
        var pagina = await fixture.NuevaPaginaAsync();
        await pagina.GotoAsync(fixture.BaseUrl);

        await pagina.GetByRole(AriaRole.Link, new() { Name = "Proveedores", Exact = true }).First.ClickAsync();

        await Assertions.Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Proveedores" })).ToBeVisibleAsync();
        await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(".*/Proveedores$"));
    }

    [Fact]
    public async Task ToggleTema_alternaEntreClaroYOscuro()
    {
        var pagina = await fixture.NuevaPaginaAsync();
        await pagina.GotoAsync(fixture.BaseUrl);

        var html = pagina.Locator("html");
        var temaInicial = await html.GetAttributeAsync("data-bs-theme");

        await pagina.GetByRole(AriaRole.Button, new() { Name = "🌓 Tema" }).ClickAsync();
        await pagina.WaitForFunctionAsync(
            "temaInicial => document.documentElement.getAttribute('data-bs-theme') !== temaInicial", temaInicial);

        var temaTrasClick = await html.GetAttributeAsync("data-bs-theme");
        Assert.NotEqual(temaInicial, temaTrasClick);
    }
}
