namespace Licitaciones.Application.TiposCambio;

public interface ITipoCambioAppService
{
    Task<TipoCambioDto> CrearAsync(GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken = default);

    Task<TipoCambioDto> ActualizarAsync(
        Guid id, GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken = default);

    Task<TipoCambioDto> ActivarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TipoCambioDto?> ObtenerActivoAsync(CancellationToken cancellationToken = default);

    Task<TipoCambioDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoCambioDto>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>Convierte un monto en CRC a USD usando el tipo de cambio activo (spec §8.8).</summary>
    Task<ConversionDto> ConvertirCrcAUsdAsync(decimal montoCRC, CancellationToken cancellationToken = default);
}
