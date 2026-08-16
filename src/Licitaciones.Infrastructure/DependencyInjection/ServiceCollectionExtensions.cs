using Licitaciones.Domain.Abstracciones;
using Licitaciones.Infrastructure.Persistencia;
using Licitaciones.Infrastructure.Repositorios;
using Licitaciones.Infrastructure.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra el DbContext de PostgreSQL, los repositorios, la unidad de
    /// trabajo y el reloj del sistema. La cadena de conexión se toma de
    /// <c>ConnectionStrings:LicitacionesDb</c>, sobreescribible mediante la
    /// variable de entorno <c>ConnectionStrings__LicitacionesDb</c> (Docker/Kubernetes).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LicitacionesDb")
            ?? throw new InvalidOperationException(
                "No se configuró la cadena de conexión 'ConnectionStrings:LicitacionesDb'.");

        services.AddDbContext<LicitacionesDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<ILicitacionRepository, LicitacionRepository>();
        services.AddScoped<IOfertaRepository, OfertaRepository>();
        services.AddScoped<INivelAprobacionRepository, NivelAprobacionRepository>();
        services.AddScoped<ITipoCambioRepository, TipoCambioRepository>();
        services.AddSingleton<IReloj, RelojSistema>();

        return services;
    }
}
