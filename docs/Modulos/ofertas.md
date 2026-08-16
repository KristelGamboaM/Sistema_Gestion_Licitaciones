# Módulo: Ofertas

## Propósito

Registrar, editar, listar y eliminar ofertas económicas de proveedores sobre licitaciones publicadas, aplicando las reglas de rechazo del enunciado.

## Responsabilidades por capa

- **Domain** (`Entidades/Oferta.cs`): invariante propio (monto > 0) en `Registrar` y `ActualizarMonto`. Las reglas que dependen de otras entidades (licitación, proveedor) se validan en la aplicación, no en la entidad.
- **Application** (`Ofertas/OfertaAppService.cs`): valida licitación publicada y no cerrada funcionalmente, no duplicidad por `(LicitacionId, ProveedorId)`, monto ≤ presupuesto, y que el proveedor exista y no esté eliminado; resuelve el nombre del proveedor para el DTO.
- **Web** (`Controllers/OfertasController.cs`): listado filtrable por licitación (`?licitacionId=`), alta con selector de proveedor, edición de monto, eliminación con confirmación.
- **Api**: `GET/POST /api/v1/ofertas`, `GET/PUT/DELETE /api/v1/ofertas/{id}`, y los endpoints anidados `GET/POST /api/v1/licitaciones/{id}/ofertas`.

## Reglas de negocio (US-06, US-07)

1. Solo se registran ofertas sobre licitaciones en estado `Publicada` y no cerradas funcionalmente (`EsEfectivamenteCerrada`).
2. Un proveedor no puede registrar más de una oferta para la misma licitación (`oferta.duplicada`, 409).
3. El monto no puede superar el presupuesto estimado; un monto igual al presupuesto es válido.
4. Editar y eliminar reutilizan la misma verificación de "licitación disponible": una oferta de una licitación cerrada o vencida no puede tocarse.

## Errores

| Código | Situación | HTTP |
| --- | --- | --- |
| `oferta.monto.invalido` | Monto ≤ 0 | 422 |
| `oferta.monto.excede_presupuesto` | Monto > presupuesto | 422 |
| `oferta.duplicada` | Proveedor ya ofertó en esa licitación | 409 |
| `oferta.licitacion_no_publicada` | Licitación aún en Borrador | 409 |
| `oferta.licitacion_cerrada` | Licitación cerrada o vencida por fecha | 409 |

## Pruebas

- Unitarias: `Entidades/OfertaTests.cs` (invariante de monto), `Aplicacion/OfertaAppServiceTests.cs` (duplicidad, sobre presupuesto, licitación en Borrador, licitación vencida por fecha con `RelojFalso`, proveedor inexistente).
- Verificación manual end-to-end en esta iteración contra PostgreSQL real vía API: oferta válida (`201`), duplicada (`409`), sobre presupuesto (`422`), y (avanzando el reloj real hasta pasar la fecha de cierre de una licitación publicada) el rechazo de una oferta vencida (`409`, `oferta.licitacion_cerrada`), lo que confirma el comportamiento de "cierre efectivo" de extremo a extremo.
