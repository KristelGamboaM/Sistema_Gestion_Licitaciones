using System.Text.Json.Serialization;
using Licitaciones.Api.ManejoErrores;
using Licitaciones.Application.DependencyInjection;
using Licitaciones.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExcepcionesDeNegocioHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Gestión de Licitaciones — API",
        Version = "v1",
        Description = "API REST para administrar licitaciones, proveedores, ofertas, niveles de aprobación y tipo de cambio.",
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Licitaciones API v1");
    opciones.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

/// <summary>Punto de entrada expuesto para <c>WebApplicationFactory</c> en pruebas de integración/funcionales.</summary>
public partial class Program;
