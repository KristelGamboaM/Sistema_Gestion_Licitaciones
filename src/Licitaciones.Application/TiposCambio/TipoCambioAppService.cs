using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Entidades;
using Licitaciones.Domain.Excepciones;

namespace Licitaciones.Application.TiposCambio;

public sealed class TipoCambioAppService(
    ITipoCambioRepository repositorio, IUnitOfWork unitOfWork, IReloj reloj) : ITipoCambioAppService
{
    public async Task<TipoCambioDto> CrearAsync(
        GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken = default)
    {
        var tipoCambio = TipoCambio.Crear(solicitud.CRCporUSD, solicitud.FechaVigencia, reloj);
        repositorio.Agregar(tipoCambio);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(tipoCambio);
    }

    public async Task<TipoCambioDto> ActualizarAsync(
        Guid id, GuardarTipoCambioRequest solicitud, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await ObtenerEntidadAsync(id, cancellationToken);
        tipoCambio.Actualizar(solicitud.CRCporUSD, solicitud.FechaVigencia, reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(tipoCambio);
    }

    public async Task<TipoCambioDto> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tipoCambio = await ObtenerEntidadAsync(id, cancellationToken);

        // Se desactiva y se guarda en un paso separado de la activación: el
        // índice único parcial de PostgreSQL ("Activo" = true) no admite
        // comprobación diferida, así que ambas filas no pueden pasar por
        // "Activo = true" dentro del mismo lote de sentencias.
        var activoActual = await repositorio.ObtenerActivoAsync(cancellationToken);
        if (activoActual is not null && activoActual.Id != tipoCambio.Id)
        {
            activoActual.Desactivar(reloj);
            await unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        tipoCambio.Activar(reloj);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AMapa(tipoCambio);
    }

    public async Task<TipoCambioDto?> ObtenerActivoAsync(CancellationToken cancellationToken = default)
    {
        var activo = await repositorio.ObtenerActivoAsync(cancellationToken);
        return activo is null ? null : AMapa(activo);
    }

    public async Task<TipoCambioDto> ObtenerAsync(Guid id, CancellationToken cancellationToken = default) =>
        AMapa(await ObtenerEntidadAsync(id, cancellationToken));

    public async Task<IReadOnlyList<TipoCambioDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var tipos = await repositorio.ListarTodosAsync(cancellationToken);
        return tipos.Select(AMapa).ToList();
    }

    public async Task<ConversionDto> ConvertirCrcAUsdAsync(decimal montoCRC, CancellationToken cancellationToken = default)
    {
        var activo = await repositorio.ObtenerActivoAsync(cancellationToken);
        if (activo is null)
        {
            throw new ReglaNegocioException(
                "tipo_cambio.sin_activo",
                "No hay un tipo de cambio activo configurado para realizar la conversión.");
        }

        var montoUsd = activo.ConvertirCrcAUsd(montoCRC);
        return new ConversionDto(montoCRC, montoUsd, activo.CRCporUSD, activo.FechaVigencia);
    }

    private async Task<TipoCambio> ObtenerEntidadAsync(Guid id, CancellationToken cancellationToken)
    {
        var tipoCambio = await repositorio.ObtenerPorIdAsync(id, cancellationToken);
        if (tipoCambio is null)
            throw new EntidadNoEncontradaException("Tipo de cambio", id);

        return tipoCambio;
    }

    private static TipoCambioDto AMapa(TipoCambio tipoCambio) => new(
        tipoCambio.Id, tipoCambio.CRCporUSD, tipoCambio.FechaVigencia, tipoCambio.Activo,
        tipoCambio.CreatedAt, tipoCambio.UpdatedAt);
}
