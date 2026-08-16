# Módulo: API REST (`Licitaciones.Api`)

## Propósito

Exponer todas las operaciones de negocio del sistema mediante una API REST versionada, independiente de la interfaz Web, para integración con sistemas externos (spec §5.3 paso 10: "Realizar las operaciones equivalentes mediante la API REST").

## Responsabilidades

- **Versionado:** todas las rutas bajo `/api/v1/...`.
- **Contratos:** los controladores reciben/devuelven los mismos DTO de `Licitaciones.Application` que usa la Web (`ProveedorDto`, `LicitacionDto`, `OfertaDto`, `NivelAprobacionDto`, `TipoCambioDto`), nunca entidades de Entity Framework Core.
- **Documentación interactiva:** Swagger UI (Swashbuckle) en `/swagger`, generado a partir de los mismos controladores y con anotaciones `[ProducesResponseType]` por cada código HTTP posible.
- **Errores controlados:** `ExcepcionesDeNegocioHandler` (`ManejoErrores/`) implementa `IExceptionHandler` y traduce `ReglaNegocioException`/`EntidadNoEncontradaException` a `ProblemDetails`, con código de negocio y `TraceIdentifier` como correlación, nunca una traza de pila ni un mensaje técnico interno.
- **Enums como texto:** `JsonStringEnumConverter` registrado globalmente, para que `{"accion":"Publicar"}` funcione en vez de exigir el valor numérico del enum.

## Dependencias

`Licitaciones.Infrastructure` (vía `AddInfrastructure`) y `Licitaciones.Application` (vía `AddApplication`), las mismas raíces de composición que `Licitaciones.Web`, cableadas de forma independiente en su propio `Program.cs`.

## Entradas y salidas

Ver [api.md](../api.md) para el detalle completo de endpoints, contratos y ejemplos, y [api-requests.http](../api-requests.http) para una colección reproducible de solicitudes.

## Errores

Tabla general de mapeo `TipoErrorNegocio` → HTTP en [api.md](../api.md#errores-problemdetails). Cada módulo documenta sus propios códigos de error específicos en su respectivo archivo (por ejemplo, [ofertas.md](ofertas.md#errores)).

## Pruebas

Cubierta principalmente por la verificación manual end-to-end documentada en la bitácora de cada iteración (creación, códigos 201/409/422, Swagger accesible) y por las pruebas de integración que ejercitan los mismos servicios de aplicación que consumen los controladores de la API. No se duplican pruebas de contrato HTTP línea por línea porque los controladores son delgados: delegan toda la lógica a `Licitaciones.Application`, ya cubierta por `tests/Licitaciones.UnitTests/Aplicacion`.
