using Licitaciones.Domain.Entidades;

namespace Licitaciones.Infrastructure.Persistencia;

/// <summary>
/// Datos semilla aplicados por migración: los tres niveles de aprobación del
/// enunciado (spec §8.7) y un tipo de cambio inicial administrado localmente
/// (spec §8.8), para que el sistema sea usable sin configuración manual previa.
/// </summary>
public static class DatosSemilla
{
    private static readonly DateTimeOffset FechaSemilla = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly Guid NivelEncargadoAreaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NivelGerenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid NivelJuntaDirectivaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TipoCambioInicialId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static IEnumerable<object> NivelesAprobacion() =>
    [
        new
        {
            Id = NivelEncargadoAreaId,
            MontoMinimoCRC = 0.01m,
            MontoMaximoCRC = (decimal?)999_999.99m,
            Aprobador = "Encargado de área",
            CreatedAt = FechaSemilla,
            UpdatedAt = FechaSemilla,
        },
        new
        {
            Id = NivelGerenciaId,
            MontoMinimoCRC = 1_000_000.00m,
            MontoMaximoCRC = (decimal?)9_999_999.99m,
            Aprobador = "Gerencia",
            CreatedAt = FechaSemilla,
            UpdatedAt = FechaSemilla,
        },
        new
        {
            Id = NivelJuntaDirectivaId,
            MontoMinimoCRC = 10_000_000.00m,
            MontoMaximoCRC = (decimal?)null,
            Aprobador = "Junta Directiva",
            CreatedAt = FechaSemilla,
            UpdatedAt = FechaSemilla,
        },
    ];

    public static IEnumerable<object> TiposCambio() =>
    [
        new
        {
            Id = TipoCambioInicialId,
            CRCporUSD = 520.00m,
            FechaVigencia = FechaSemilla,
            Activo = true,
            CreatedAt = FechaSemilla,
            UpdatedAt = FechaSemilla,
        },
    ];
}
