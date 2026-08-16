# Módulo: Licitaciones

## Propósito

Administrar el ciclo de vida de una licitación: creación en `Borrador`, publicación, cierre, y consulta de la mejor oferta con su clasificación y nivel de aprobación.

## Responsabilidades por capa

- **Domain** (`Entidades/Licitacion.cs`): máquina de estados (`Borrador → Publicada → Cerrada`), cierre efectivo por fecha vencida, invariantes de código/título/presupuesto.
- **Application** (`Licitaciones/LicitacionAppService.cs`): valida unicidad de código, orquesta `ILicitacionRepository` + `IOfertaRepository` + `INivelAprobacionRepository` + `IProveedorRepository` para resolver la mejor oferta con nombre de proveedor y aprobador en una sola consulta.
- **Web**: listado con estado (badge), detalle con acciones Publicar/Cerrar y tarjeta de mejor oferta, formularios con selector de fecha/hora (`type="datetime-local"`).
- **Api**: `GET/POST /api/v1/licitaciones`, `GET/PUT/DELETE /api/v1/licitaciones/{id}`, `PATCH /api/v1/licitaciones/{id}/estado` (`{"accion":"Publicar"|"Cerrar"}`), `GET /api/v1/licitaciones/{id}/mejor-oferta`.

## Manejo de fechas (spec §8.2)

Las comparaciones de negocio ocurren siempre en UTC. La interfaz MVC captura/muestra en `America/Costa_Rica` (`Licitaciones.Web.Comun.ZonaHorariaCostaRica`, offset fijo -06:00, sin horario de verano). Como defensa adicional a nivel de persistencia (para cubrir también las solicitudes que llegan directamente a la API con cualquier offset), `LicitacionesDbContext` aplica una conversión global (`DateTimeOffsetUtcConverter`) que normaliza **todo** `DateTimeOffset` a UTC antes de escribirlo, porque Npgsql solo acepta escribir `timestamptz` con offset 0.

## Reglas de negocio (US-03, US-04, US-05, US-08)

1. Código único ignorando trim/case; presupuesto > 0; fecha de cierre se captura con calendario/hora.
2. `Borrador → Publicada` solo si la fecha de cierre es futura respecto al reloj inyectado.
3. Una licitación con `FechaCierre` ya alcanzada se considera **cerrada funcionalmente** (`EsEfectivamenteCerrada`) aunque el campo `Estado` siga en `Publicada`; esto se refleja en la UI con la etiqueta "Publicada (vencida)".
4. Solo se edita en `Borrador`; el presupuesto no puede bajar de una oferta ya registrada (defensa en profundidad, aunque en la práctica no puede haber ofertas en `Borrador`).
5. Mejor oferta = menor monto válido (empate → primera registrada); clasificación según `CalculadoraMejorOferta` del dominio.

## Errores

| Código | Origen | HTTP |
| --- | --- | --- |
| `licitacion.codigo.duplicado` | Aplicación | 409 |
| `licitacion.presupuesto.invalido` / `.reduccion_invalida` | Dominio | 422 |
| `licitacion.fecha_cierre.pasada` | Dominio (publicar) | 422 |
| `licitacion.transicion.invalida` | Dominio | 422 |
| `licitacion.edicion.no_permitida` | Dominio (editar fuera de Borrador) | 422 |

## Pruebas

- Unitarias: `Entidades/LicitacionTests.cs` (máquina de estados, cierre efectivo, presupuesto), `Aplicacion/LicitacionAppServiceTests.cs` (orquestación con repositorios simulados, mejor oferta con aprobador y nombre de proveedor resueltos).
- Verificación manual end-to-end en esta iteración contra PostgreSQL real: creación, publicación, código duplicado (`409`), consulta de mejor oferta ("Sin ofertas válidas"), probado vía MVC (formulario con `datetime-local`) y API (`PATCH .../estado`).
