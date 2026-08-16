using Licitaciones.Domain.Abstracciones;

namespace Licitaciones.Infrastructure.Servicios;

public sealed class RelojSistema : IReloj
{
    public DateTimeOffset UtcAhora => DateTimeOffset.UtcNow;
}
