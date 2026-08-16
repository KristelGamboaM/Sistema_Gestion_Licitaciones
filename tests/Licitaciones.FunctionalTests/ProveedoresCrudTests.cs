using Licitaciones.FunctionalTests.Comunes;
using Microsoft.Playwright;

namespace Licitaciones.FunctionalTests;

/// <summary>US-01/US-02: CRUD completo de proveedores desde el navegador.</summary>
[Collection(FuncionalWebCollection.NombreColeccion)]
public class ProveedoresCrudTests(FuncionalWebFixture fixture)
{
    [Fact]
    public async Task CrearEditarYEliminarProveedor_flujoCompletoDesdeElNavegador()
    {
        var nombre = $"Constructora Playwright {Guid.NewGuid():N}"[..40];
        var nombreEditado = $"{nombre} Editada";

        var pagina = await fixture.NuevaPaginaAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombre);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(nombre)).ToBeVisibleAsync();

        await pagina.GetByRole(AriaRole.Row, new() { Name = nombre })
            .GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombreEditado);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(nombreEditado)).ToBeVisibleAsync();

        await pagina.GetByRole(AriaRole.Row, new() { Name = nombreEditado })
            .GetByRole(AriaRole.Link, new() { Name = "Eliminar" }).ClickAsync();
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Sí, eliminar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText(nombreEditado)).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task RegistrarProveedorDuplicado_muestraMensajeDeValidacion()
    {
        var nombre = $"Proveedor Duplicado {Guid.NewGuid():N}"[..40];

        var pagina = await fixture.NuevaPaginaAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombre);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByLabel("Nombre del proveedor").FillAsync(nombre.ToUpperInvariant());
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText("Ya existe un proveedor registrado con ese nombre."))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task FormularioVacio_muestraValidacionDeCampoRequerido()
    {
        var pagina = await fixture.NuevaPaginaAsync();

        await pagina.GotoAsync($"{fixture.BaseUrl}/Proveedores/Create");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar" }).ClickAsync();

        await Assertions.Expect(pagina.GetByText("El nombre es obligatorio.")).ToBeVisibleAsync();
    }
}
