# API REST

## Convenciones generales

- Base: `/api/v1/...`, versionada en la ruta.
- Formato: `application/json`. Los DTO de `Licitaciones.Application` son los contratos de transporte; **nunca se exponen entidades de Entity Framework Core**.
- Enums (por ejemplo, `accion` de la transición de estado) se serializan como texto (`JsonStringEnumConverter`), no como números.
- Documentación interactiva: Swagger UI en `/swagger` (generado con Swashbuckle a partir de los mismos controladores).
- Colección reproducible de solicitudes: [api-requests.http](api-requests.http), ejecutable con la extensión REST Client de VS Code o copiando cada bloque a `curl`.

## Errores (`ProblemDetails`)

Toda excepción de negocio se traduce por `Licitaciones.Api.ManejoErrores.ExcepcionesDeNegocioHandler` a un cuerpo `ProblemDetails` estándar (RFC 9457), nunca a una traza de pila:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Regla de negocio incumplida",
  "status": 409,
  "detail": "El proveedor ya registró una oferta para esta licitación.",
  "codigo": "oferta.duplicada",
  "correlacionId": "0HNNMFMM22K8H:00000001"
}
```

| Situación | HTTP | `codigo` (ejemplos) |
| --- | --- | --- |
| Entidad no encontrada | 404 | — (`title`: "Recurso no encontrado") |
| Violación de regla con severidad "Conflicto" (duplicados, traslapes, cierre) | 409 | `*.duplicado`, `*.traslape`, `oferta.licitacion_cerrada` |
| Violación de regla con severidad "Validación" (montos, campos requeridos) | 422 | `*.invalido`, `*.requerido` |
| Error de validación de modelo (`[ApiController]` automático) | 400 | — |

`codigo` es estable y apto para manejo programático por el cliente; `correlacionId` corresponde al `TraceIdentifier` de la solicitud, útil para correlacionar con los logs del servidor sin exponer detalles internos.

## Endpoints

### Proveedores

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/proveedores?busqueda=&incluirEliminados=&pagina=&tamanoPagina=&descendente=` | Lista paginada, filtrable por nombre |
| GET | `/proveedores/{id}` | Detalle |
| POST | `/proveedores` | Crear (`{ "nombre": "..." }`) |
| PUT | `/proveedores/{id}` | Editar |
| DELETE | `/proveedores/{id}` | Borrado lógico |

### Licitaciones

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/licitaciones?busqueda=&pagina=&tamanoPagina=&descendente=&ordenarPor=` | Lista paginada. `ordenarPor`: `FechaCierre` (por defecto), `Codigo`, `Titulo`, `Presupuesto`, `Estado` |
| GET | `/licitaciones/{id}` | Detalle |
| POST | `/licitaciones` | Crear (`Borrador`) |
| PUT | `/licitaciones/{id}` | Editar (solo en `Borrador`) |
| PATCH | `/licitaciones/{id}/estado` | Transición: `{ "accion": "Publicar" }` o `{ "accion": "Cerrar" }` |
| DELETE | `/licitaciones/{id}` | Borrado lógico |
| GET | `/licitaciones/{id}/ofertas` | Ofertas de la licitación (paginado) |
| POST | `/licitaciones/{id}/ofertas` | Registrar oferta (`{ "proveedorId": "...", "montoOfertadoCRC": ... }`) |
| GET | `/licitaciones/{id}/mejor-oferta` | Mejor oferta, ahorro, clasificación y aprobador |

### Ofertas

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/ofertas?licitacionId=&proveedorId=&pagina=&tamanoPagina=&descendente=&ordenarPor=` | Lista filtrable. `ordenarPor`: `FechaRegistro` (por defecto) o `Monto` |
| GET | `/ofertas/{id}` | Detalle |
| POST | `/ofertas` | Registrar (equivalente a `POST /licitaciones/{id}/ofertas`) |
| PUT | `/ofertas/{id}` | Editar monto |
| DELETE | `/ofertas/{id}` | Eliminar (solo si la licitación no está cerrada) |

### Niveles de aprobación

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/niveles-aprobacion` | Lista completa (ordenada por monto mínimo) |
| GET | `/niveles-aprobacion/{id}` | Detalle |
| POST | `/niveles-aprobacion` | Crear (valida ausencia de traslape) |
| PUT | `/niveles-aprobacion/{id}` | Editar |
| DELETE | `/niveles-aprobacion/{id}` | Eliminar |

### Tipos de cambio

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/tipos-cambio` | Lista completa |
| GET | `/tipos-cambio/activo` | Tipo de cambio actualmente activo |
| GET | `/tipos-cambio/{id}` | Detalle |
| POST | `/tipos-cambio` | Crear (queda inactivo por defecto) |
| PUT | `/tipos-cambio/{id}` | Editar |
| PATCH | `/tipos-cambio/{id}/activar` | Activar (desactiva automáticamente el anterior) |
| GET | `/tipos-cambio/convertir?montoCRC=` | Convierte un monto a USD con la tasa activa |

## Ejemplo completo

Ver [api-requests.http](api-requests.http) para el flujo de extremo a extremo: crear proveedor → crear licitación → publicar → registrar oferta → rechazar duplicada (409) → rechazar sobre presupuesto (422) → consultar mejor oferta → cerrar licitación.
