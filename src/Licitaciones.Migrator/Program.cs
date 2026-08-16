using Licitaciones.Infrastructure.DependencyInjection;
using Licitaciones.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Ejecuta las migraciones pendientes de EF Core como un paso controlado y
// separado del arranque de la aplicación (spec §13.2: "Migraciones
// ejecutadas de forma controlada") — se usa como initContainer/Job en
// Kubernetes y como servicio de un solo uso en Docker Compose.
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var contexto = scope.ServiceProvider.GetRequiredService<LicitacionesDbContext>();

Console.WriteLine("Aplicando migraciones pendientes...");
await contexto.Database.MigrateAsync();
Console.WriteLine("Migraciones aplicadas correctamente.");
