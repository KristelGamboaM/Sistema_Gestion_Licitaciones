using Licitaciones.FunctionalTests.Comunes;
using Microsoft.Playwright;

namespace Licitaciones.FunctionalTests;

/// <summary>US-12: alternar montos entre CRC y USD sin alterar los valores originales.</summary>
[Collection(FuncionalWebCollection.NombreColeccion)]
public class ToggleMonedaTests(FuncionalWebFixture fixture)
{
    [Fact]
    public async Task ToggleMoneda_cambiaLosMontosMostradosDeCrcAUsd()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var codigoLicitacion = $"LIC-USD-{sufijo}";

        var pagina = await fixture.NuevaPaginaAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await pagina.GetByLabel("Código").FillAsync(codigoLicitacion);
        await pagina.GetByLabel("Título").FillAsync("Licitación para probar el toggle de moneda");
        await pagina.GetByLabel("Presupuesto estimado (CRC)").FillAsync("1000000");
        await pagina.Locator("input[type='datetime-local']").FillAsync("2030-12-31T10:00");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        var fila = pagina.GetByRole(AriaRole.Row, new() { Name = codigoLicitacion });
        await Assertions.Expect(fila.GetByText("₡")).ToBeVisibleAsync();

        await pagina.GetByRole(AriaRole.Button, new() { Name = "Ver en $ USD" }).ClickAsync();

        await Assertions.Expect(fila.GetByText("$")).ToBeVisibleAsync();
        await Assertions.Expect(pagina.GetByRole(AriaRole.Button, new() { Name = "Ver en ₡ CRC" })).ToBeVisibleAsync();
    }
}
