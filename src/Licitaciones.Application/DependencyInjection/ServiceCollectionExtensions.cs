using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProveedorAppService, ProveedorAppService>();
        services.AddScoped<ILicitacionAppService, LicitacionAppService>();
        services.AddScoped<IOfertaAppService, OfertaAppService>();
        services.AddScoped<INivelAprobacionAppService, NivelAprobacionAppService>();
        services.AddScoped<ITipoCambioAppService, TipoCambioAppService>();
        return services;
    }
}
