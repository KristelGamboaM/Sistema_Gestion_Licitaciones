using Licitaciones.FunctionalTests.Comunes;
using Microsoft.Playwright;

namespace Licitaciones.FunctionalTests;

/// <summary>
/// US-03/US-04/US-06/US-07/US-08: flujo funcional mínimo del enunciado
/// (§5.3) — crear licitación con calendario, publicar, ofertar, rechazar
/// duplicada y sobre presupuesto, y consultar la mejor oferta.
/// </summary>
[Collection(FuncionalWebCollection.NombreColeccion)]
public class LicitacionesOfertasFlujoTests(FuncionalWebFixture fixture)
{
    [Fact]
    public async Task FlujoCompleto_crearPublicarOfertarYConsultarMejorOferta()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var codigoLicitacion = $"LIC-PW-{sufijo}";
        var nombreProveedor = $"Proveedor Flujo {sufijo}";

        var pagina = await fixture.NuevaPaginaAsync();

        // Proveedor
        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombreProveedor);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        // Licitación (calendario/hora, no solo texto libre — spec §8.2)
        await pagina.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await pagina.GetByLabel("Código").FillAsync(codigoLicitacion);
        await pagina.GetByLabel("Título").FillAsync("Compra de equipo — prueba funcional");
        await pagina.GetByLabel("Presupuesto estimado (CRC)").FillAsync("1000000");
        await pagina.Locator("input[type='datetime-local']").FillAsync("2030-12-31T10:00");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(codigoLicitacion)).ToBeVisibleAsync();
        await pagina.GetByRole(AriaRole.Row, new() { Name = codigoLicitacion })
            .GetByRole(AriaRole.Link, new() { Name = "Ver" }).ClickAsync();

        // Publicar
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Publicar" }).ClickAsync();
        await Assertions.Expect(pagina.GetByText("Publicada").First).ToBeVisibleAsync();

        // Registrar oferta válida
        await pagina.GetByRole(AriaRole.Link, new() { Name = "Ver ofertas" }).ClickAsync();
        await pagina.GetByRole(AriaRole.Link, new() { Name = "Nueva oferta" }).ClickAsync();
        await pagina.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = nombreProveedor });
        await pagina.GetByLabel("Monto ofertado (CRC)").FillAsync("900000");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(nombreProveedor)).ToBeVisibleAsync();

        // Rechazo: oferta duplicada del mismo proveedor
        await pagina.GetByRole(AriaRole.Link, new() { Name = "Nueva oferta" }).ClickAsync();
        await pagina.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = nombreProveedor });
        await pagina.GetByLabel("Monto ofertado (CRC)").FillAsync("800000");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText("El proveedor ya registró una oferta para esta licitación."))
            .ToBeVisibleAsync();

        // Consultar mejor oferta desde el detalle de la licitación
        await pagina.GotoAsync($"{fixture.BaseUrl}/Licitaciones");
        await pagina.GetByRole(AriaRole.Row, new() { Name = codigoLicitacion })
            .GetByRole(AriaRole.Link, new() { Name = "Ver" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(nombreProveedor)).ToBeVisibleAsync();
        await Assertions.Expect(pagina.GetByText("Oferta conveniente")).ToBeVisibleAsync();
        await Assertions.Expect(pagina.GetByText("Gerencia")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RegistrarOferta_sobrePresupuesto_muestraMensajeDeRechazo()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var codigoLicitacion = $"LIC-PW-B-{sufijo}";
        var nombreProveedor = $"Proveedor Presupuesto {sufijo}";

        var pagina = await fixture.NuevaPaginaAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombreProveedor);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Licitaciones/Create");
        await pagina.GetByLabel("Código").FillAsync(codigoLicitacion);
        await pagina.GetByLabel("Título").FillAsync("Licitación de presupuesto ajustado");
        await pagina.GetByLabel("Presupuesto estimado (CRC)").FillAsync("500000");
        await pagina.Locator("input[type='datetime-local']").FillAsync("2030-12-31T10:00");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await pagina.GetByRole(AriaRole.Row, new() { Name = codigoLicitacion })
            .GetByRole(AriaRole.Link, new() { Name = "Ver" }).ClickAsync();
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Publicar" }).ClickAsync();

        await pagina.GetByRole(AriaRole.Link, new() { Name = "Ver ofertas" }).ClickAsync();
        await pagina.GetByRole(AriaRole.Link, new() { Name = "Nueva oferta" }).ClickAsync();
        await pagina.GetByLabel("Proveedor").SelectOptionAsync(new SelectOptionValue { Label = nombreProveedor });
        await pagina.GetByLabel("Monto ofertado (CRC)").FillAsync("900000");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(
                pagina.GetByText("El monto ofertado no puede superar el presupuesto estimado de la licitación."))
            .ToBeVisibleAsync();
    }
}
