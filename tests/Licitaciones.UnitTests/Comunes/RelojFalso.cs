using Licitaciones.Domain.Abstracciones;

namespace Licitaciones.UnitTests.Comunes;

/// <summary>Reloj determinista para pruebas: fija un instante y permite avanzarlo explícitamente.</summary>
public sealed class RelojFalso(DateTimeOffset instanteInicial) : IReloj
{
    public DateTimeOffset UtcAhora { get; private set; } = instanteInicial;

    public static RelojFalso EnUtc(int anio, int mes, int dia, int hora = 0, int minuto = 0) =>
        new(new DateTimeOffset(anio, mes, dia, hora, minuto, 0, TimeSpan.Zero));

    public void AvanzarA(DateTimeOffset nuevoInstante) => UtcAhora = nuevoInstante;
}
