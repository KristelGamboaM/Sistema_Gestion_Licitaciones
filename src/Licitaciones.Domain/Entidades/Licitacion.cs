using Licitaciones.Domain.Abstracciones;
using Licitaciones.Domain.Comun;
using Licitaciones.Domain.Excepciones;
using Licitaciones.Domain.Servicios;

namespace Licitaciones.Domain.Entidades;

public sealed class Licitacion : EntidadBase
{
    public string Codigo { get; private set; } = string.Empty;
    public string CodigoNormalizado { get; private set; } = string.Empty;
    public string Titulo { get; private set; } = string.Empty;
    public EstadoLicitacion Estado { get; private set; }
    public DateTimeOffset FechaCierre { get; private set; }
    public decimal PresupuestoEstimadoCRC { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool EstaEliminada => DeletedAt is not null;

    private Licitacion()
    {
        // Requerido por EF Core.
    }

    public static Licitacion Crear(
        string codigo,
        string titulo,
        decimal presupuestoEstimadoCRC,
        DateTimeOffset fechaCierre,
        IReloj reloj)
    {
        var licitacion = new Licitacion
        {
            CreatedAt = reloj.UtcAhora,
            Estado = EstadoLicitacion.Borrador,
        };

        licitacion.EstablecerCodigo(codigo);
        licitacion.EstablecerTitulo(titulo);
        licitacion.EstablecerPresupuesto(presupuestoEstimadoCRC, montoMayorOfertaExistente: null);
        licitacion.FechaCierre = fechaCierre;
        licitacion.UpdatedAt = reloj.UtcAhora;
        return licitacion;
    }

    /// <summary>
    /// Una licitación cuya fecha de cierre ya pasó se trata como cerrada en
    /// toda regla de negocio, aunque el campo Estado no se haya actualizado.
    /// </summary>
    public bool EsEfectivamenteCerrada(IReloj reloj) =>
        Estado == EstadoLicitacion.Cerrada || FechaCierre <= reloj.UtcAhora;

    public void Publicar(IReloj reloj)
    {
        if (EstaEliminada)
            throw new ReglaNegocioException("licitacion.eliminada", "No se puede publicar una licitación eliminada.");

        if (Estado != EstadoLicitacion.Borrador)
        {
            throw new ReglaNegocioException(
                "licitacion.transicion.invalida",
                $"No se puede publicar una licitación en estado {Estado}.");
        }

        if (FechaCierre <= reloj.UtcAhora)
        {
            throw new ReglaNegocioException(
                "licitacion.fecha_cierre.pasada",
                "La fecha de cierre debe ser futura para publicar la licitación.");
        }

        Estado = EstadoLicitacion.Publicada;
        UpdatedAt = reloj.UtcAhora;
    }

    /// <summary>Cierre manual o cancelación documentada (permitida desde Borrador o Publicada).</summary>
    public void Cerrar(IReloj reloj)
    {
        if (Estado == EstadoLicitacion.Cerrada)
            return;

        Estado = EstadoLicitacion.Cerrada;
        UpdatedAt = reloj.UtcAhora;
    }

    /// <param name="montoMayorOfertaExistente">
    /// Monto de la oferta más alta ya registrada (calculado por la capa de
    /// aplicación); se exige explícitamente para no acoplar el dominio al
    /// acceso a datos. En la práctica siempre es <c>null</c> porque solo se
    /// permite editar en Borrador y las ofertas requieren una licitación
    /// Publicada, pero se valida igual como defensa en profundidad.
    /// </param>
    public void ActualizarDatosBorrador(
        string codigo,
        string titulo,
        decimal presupuestoEstimadoCRC,
        DateTimeOffset fechaCierre,
        decimal? montoMayorOfertaExistente,
        IReloj reloj)
    {
        if (Estado != EstadoLicitacion.Borrador)
        {
            throw new ReglaNegocioException(
                "licitacion.edicion.no_permitida",
                "Solo se puede editar una licitación en estado Borrador.");
        }

        EstablecerCodigo(codigo);
        EstablecerTitulo(titulo);
        EstablecerPresupuesto(presupuestoEstimadoCRC, montoMayorOfertaExistente);
        FechaCierre = fechaCierre;
        UpdatedAt = reloj.UtcAhora;
    }

    public void Eliminar(IReloj reloj)
    {
        if (EstaEliminada)
            return;

        DeletedAt = reloj.UtcAhora;
        UpdatedAt = reloj.UtcAhora;
    }

    private void EstablecerCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ReglaNegocioException("licitacion.codigo.requerido", "El código de la licitación es obligatorio.");

        var codigoRecortado = codigo.Trim();
        Codigo = codigoRecortado;
        CodigoNormalizado = NormalizacionTexto.NormalizarCodigoLicitacion(codigoRecortado);
    }

    private void EstablecerTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ReglaNegocioException("licitacion.titulo.requerido", "El título de la licitación es obligatorio.");

        Titulo = titulo.Trim();
    }

    private void EstablecerPresupuesto(decimal presupuestoEstimadoCRC, decimal? montoMayorOfertaExistente)
    {
        if (presupuestoEstimadoCRC <= 0)
        {
            throw new ReglaNegocioException(
                "licitacion.presupuesto.invalido",
                "El presupuesto estimado debe ser mayor que cero.");
        }

        if (montoMayorOfertaExistente is not null && presupuestoEstimadoCRC < montoMayorOfertaExistente)
        {
            throw new ReglaNegocioException(
                "licitacion.presupuesto.reduccion_invalida",
                "El presupuesto no puede reducirse por debajo de una oferta ya registrada.");
        }

        PresupuestoEstimadoCRC = presupuestoEstimadoCRC;
    }
}
