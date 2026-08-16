using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licitaciones.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRestriccionesCheckMontos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_tipos_cambio_tasa_positiva",
                table: "tipos_cambio",
                sql: "\"CRCporUSD\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ofertas_monto_positivo",
                table: "ofertas",
                sql: "\"MontoOfertadoCRC\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_niveles_aprobacion_minimo_positivo",
                table: "niveles_aprobacion",
                sql: "\"MontoMinimoCRC\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_niveles_aprobacion_rango_valido",
                table: "niveles_aprobacion",
                sql: "\"MontoMaximoCRC\" IS NULL OR \"MontoMaximoCRC\" > \"MontoMinimoCRC\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_licitaciones_presupuesto_positivo",
                table: "licitaciones",
                sql: "\"PresupuestoEstimadoCRC\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_tipos_cambio_tasa_positiva",
                table: "tipos_cambio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ofertas_monto_positivo",
                table: "ofertas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_niveles_aprobacion_minimo_positivo",
                table: "niveles_aprobacion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_niveles_aprobacion_rango_valido",
                table: "niveles_aprobacion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_licitaciones_presupuesto_positivo",
                table: "licitaciones");
        }
    }
}
