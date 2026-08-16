namespace Licitaciones.Web.Comun;

/// <summary>
/// Las comparaciones de negocio se hacen en UTC; la interfaz siempre
/// presenta y captura fechas en America/Costa_Rica (UTC-6, sin horario de
/// verano), sin depender de la zona horaria del servidor donde corra la app.
/// </summary>
public static class ZonaHorariaCostaRica
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-6);

    public static DateTimeOffset ALocalCostaRica(this DateTimeOffset instante) => instante.ToOffset(Offset);

    /// <summary>
    /// Interpreta el valor de un formulario como hora local de Costa Rica y
    /// lo normaliza a UTC (offset 0): PostgreSQL/Npgsql solo acepta escribir
    /// <c>timestamptz</c> con <see cref="DateTimeOffset"/> en UTC, y el
    /// dominio compara fechas siempre en UTC (spec §8.2).
    /// </summary>
    public static DateTimeOffset DesdeFormularioLocal(DateTime fechaHoraLocal) =>
        new DateTimeOffset(fechaHoraLocal, Offset).ToUniversalTime();
}
