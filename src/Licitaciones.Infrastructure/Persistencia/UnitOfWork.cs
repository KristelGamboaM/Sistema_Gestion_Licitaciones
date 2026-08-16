using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistencia;

public sealed class UnitOfWork(LicitacionesDbContext contexto) : IUnitOfWork
{
    private const string CodigoViolacionUnica = "23505";
    private const string CodigoViolacionLlaveForanea = "23503";

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ReglaNegocioException(
                "concurrencia.conflicto",
                "El registro fue modificado por otra persona antes de guardar los cambios. Recargue e intente de nuevo.",
                TipoErrorNegocio.Conflicto);
        }
        catch (DbUpdateException excepcion) when (excepcion.InnerException is PostgresException postgres)
        {
            throw TraducirErrorPostgres(postgres);
        }
    }

    private static ReglaNegocioException TraducirErrorPostgres(PostgresException postgres) => postgres.SqlState switch
    {
        CodigoViolacionUnica => new ReglaNegocioException(
            "integridad.duplicado",
            "Ya existe un registro con ese valor único.",
            TipoErrorNegocio.Conflicto),
        CodigoViolacionLlaveForanea => new ReglaNegocioException(
            "integridad.referencia_invalida",
            "La operación hace referencia a un registro relacionado que no existe o no puede modificarse.",
            TipoErrorNegocio.Conflicto),
        _ => new ReglaNegocioException(
            "integridad.error",
            "No fue posible completar la operación por una restricción de integridad de datos.",
            TipoErrorNegocio.Conflicto),
    };
}
