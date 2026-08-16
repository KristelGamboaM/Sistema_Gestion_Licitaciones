# Módulo: Persistencia (`Licitaciones.Infrastructure`)

## Propósito

Implementar el acceso a datos sobre PostgreSQL 16 mediante Entity Framework Core 9, traduciendo las abstracciones definidas en `Licitaciones.Domain` (`ILicitacionRepository`, `IProveedorRepository`, `IOfertaRepository`, `INivelAprobacionRepository`, `ITipoCambioRepository`, `IUnitOfWork`) a operaciones reales de base de datos, sin filtrar detalles de EF Core hacia el dominio o la aplicación.

## Responsabilidades

- `LicitacionesDbContext`: `DbContext` con un `DbSet<T>` por entidad; carga las configuraciones vía `ApplyConfigurationsFromAssembly`.
- Configuraciones (`Persistencia/Configuraciones/*Configuracion.cs`): mapeo de tablas, longitudes, tipos `numeric`, índices únicos, claves foráneas, restricciones `CHECK` y datos semilla, una clase `IEntityTypeConfiguration<T>` por entidad.
- Repositorios (`Repositorios/*Repository.cs`): implementan las interfaces del dominio usando LINQ sobre el `DbContext`; devuelven `PaginaResultado<T>` para listados con paginación/filtro/orden.
- `UnitOfWork`: implementa `IUnitOfWork.GuardarCambiosAsync()`, envolviendo `DbContext.SaveChangesAsync()` y traduciendo excepciones técnicas a `ReglaNegocioException` controladas.
- `RelojSistema`: implementación de `IReloj` basada en `DateTimeOffset.UtcNow` para producción (las pruebas usan `RelojFalso`).
- `DatosSemilla`: niveles de aprobación y tipo de cambio inicial, aplicados por migración.
- `LicitacionesDbContextFactory`: fábrica de tiempo de diseño para que `dotnet ef migrations` funcione sin levantar Web/Api.

## Dependencias

- `Licitaciones.Domain` (abstracciones e entidades) y `Licitaciones.Application` (vía la referencia de proyecto existente).
- `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4, `Microsoft.EntityFrameworkCore` / `.Relational` 9.0.4, `Microsoft.EntityFrameworkCore.Design` 9.0.4 (solo herramientas, `PrivateAssets="all"`).
- Las versiones de estos paquetes se centralizan en `Directory.Packages.props` (Central Package Management) para evitar conflictos de versión entre proyectos que consumen `Licitaciones.Infrastructure` transitivamente (Web, Api).

## Entradas y salidas

- **Entrada:** cadena de conexión en `ConnectionStrings:LicitacionesDb`, resuelta por configuración estándar de .NET: `appsettings.Development.json` en desarrollo local, variable de entorno `ConnectionStrings__LicitacionesDb` en Docker/Kubernetes. Nunca se versionan credenciales reales.
- **Salida:** entidades de dominio materializadas (nunca DTOs ni tipos de EF Core expuestos fuera de esta capa).

## Concurrencia optimista

Se evaluó `UseXminAsConcurrencyToken()` (documentado en versiones antiguas del proveedor Npgsql) pero **no existe** en `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4 (verificado por inspección del ensamblado). Se usa en su lugar el mecanismo portable y soportado de EF Core: una propiedad sombra `uint Version` mapeada a la columna de sistema `xmin` con `ValueGeneratedOnAddOrUpdate()` + `IsConcurrencyToken()` (`Persistencia/Extensiones/EntityTypeBuilderExtensiones.cs`, método `UsarXminComoTokenConcurrencia`), aplicada a las cinco entidades. Una edición concurrente lanza `DbUpdateConcurrencyException`, capturada por `UnitOfWork` y traducida a `ReglaNegocioException` (`concurrencia.conflicto`, `TipoErrorNegocio.Conflicto`).

## Errores

`UnitOfWork.GuardarCambiosAsync()` traduce:

| Excepción técnica | Código de negocio | HTTP sugerido |
| --- | --- | --- |
| `DbUpdateConcurrencyException` | `concurrencia.conflicto` | 409 |
| `DbUpdateException` (Postgres `23505`, violación de unicidad) | `integridad.duplicado` | 409 |
| `DbUpdateException` (Postgres `23503`, violación de llave foránea) | `integridad.referencia_invalida` | 409 |
| `DbUpdateException` (otra restricción) | `integridad.error` | 409 |

Ninguna capa superior necesita inspeccionar `PostgresException` directamente.

## Restricciones CHECK

Además de la validación en dominio/aplicación (spec §8.5), PostgreSQL rechaza en la propia base de datos los montos que no sean estrictamente positivos, como defensa en profundidad:

| Tabla | Restricción | Regla |
| --- | --- | --- |
| `licitaciones` | `CK_licitaciones_presupuesto_positivo` | `PresupuestoEstimadoCRC > 0` |
| `ofertas` | `CK_ofertas_monto_positivo` | `MontoOfertadoCRC > 0` |
| `tipos_cambio` | `CK_tipos_cambio_tasa_positiva` | `CRCporUSD > 0` |
| `niveles_aprobacion` | `CK_niveles_aprobacion_minimo_positivo` | `MontoMinimoCRC > 0` |
| `niveles_aprobacion` | `CK_niveles_aprobacion_rango_valido` | `MontoMaximoCRC IS NULL OR MontoMaximoCRC > MontoMinimoCRC` |

Una violación de `CHECK` llega como `DbUpdateException` (Postgres `23514`) y cae en la rama genérica `integridad.error` de la tabla de errores anterior.

## Pruebas

- **Integración** (`tests/Licitaciones.IntegrationTests/Persistencia/`), contra PostgreSQL 16 real levantado con Testcontainers (no SQLite, no InMemory):
  - `MigracionesYSemillaTests`: las migraciones quedan aplicadas y la semilla contiene los tres niveles de aprobación y un tipo de cambio activo.
  - `IndicesUnicosTests`: violación de los índices únicos de proveedor, licitación y oferta compuesta, y su traducción a `ReglaNegocioException` vía `UnitOfWork`.
  - `ConcurrenciaOptimistaTests`: dos contextos editando el mismo proveedor producen un conflicto de concurrencia detectado por `xmin`.

## Cómo aplicar migraciones localmente

```bash
dotnet tool install --global dotnet-ef
export ConnectionStrings__LicitacionesDb="Host=localhost;Port=5432;Database=licitaciones;Username=licitaciones;Password=licitaciones_dev"
dotnet ef database update --project src/Licitaciones.Infrastructure --startup-project src/Licitaciones.Infrastructure
```

Verificado end-to-end en esta iteración contra un contenedor `postgres:16` real: esquema, índices, llaves foráneas y datos semilla quedaron exactamente como se describe en [modelo-datos.md](../modelo-datos.md).
