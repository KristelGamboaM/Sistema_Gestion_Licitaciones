using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Licitaciones.Infrastructure.Persistencia.Conversiones;

/// <summary>
/// Npgsql solo admite escribir <c>timestamptz</c> con <see cref="DateTimeOffset"/>
/// en UTC (offset 0). Se normaliza aquí, a nivel de persistencia, para que
/// ningún origen (formulario MVC, API, pruebas) tenga que recordarlo caso
/// por caso — coherente con la regla de comparar fechas siempre en UTC
/// (spec §8.2).
/// </summary>
public sealed class DateTimeOffsetUtcConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(v => v.ToUniversalTime(), v => v);
