# Módulo: Proveedores

## Propósito

Administrar el catálogo de proveedores que pueden ofertar en licitaciones, garantizando nombres únicos y bien formados.

## Responsabilidades por capa

- **Domain** (`Entidades/Proveedor.cs`): invariantes de nombre obligatorio, caracteres permitidos (`^[\p{L}\p{N} .,()]+$`), normalización para unicidad, borrado lógico idempotente.
- **Application** (`Proveedores/ProveedorAppService.cs`): orquesta creación/edición validando unicidad contra el repositorio antes de tocar el dominio (mensaje de conflicto claro, además de la defensa en profundidad del índice único en PostgreSQL), mapea `Proveedor` → `ProveedorDto`.
- **Infrastructure** (`Repositorios/ProveedorRepository.cs`): consulta paginada con búsqueda por nombre normalizado, índice único `NombreNormalizado`.
- **Web** (`Controllers/ProveedoresController.cs` + `Views/Proveedores/*`): listado con búsqueda y paginación, alta, edición y eliminación con confirmación; validación de formulario con DataAnnotations + jQuery Validation Unobtrusive (cliente) y `ReglaNegocioException` (servidor).
- **Api** (`Controllers/ProveedoresController.cs`): `GET/POST /api/v1/proveedores`, `GET/PUT/DELETE /api/v1/proveedores/{id}`.

## Entradas y salidas

- **Entrada:** `Nombre` (texto).
- **Salida:** `ProveedorDto { Id, Nombre, CreatedAt, UpdatedAt }`.

## Reglas de negocio (US-01, US-02)

1. El nombre se normaliza (trim, colapso de espacios, NFKC, mayúsculas) para comparar unicidad; `Empresa Central`, `empresa central` y `EMPRESA CENTRAL` son el mismo proveedor.
2. Solo letras, números, espacios, punto, coma y paréntesis.
3. Eliminar aplica borrado lógico (`DeletedAt`); un proveedor eliminado no aparece en listados activos ni puede recibir nuevas ofertas.

## Errores

| Código | Origen | HTTP |
| --- | --- | --- |
| `proveedor.nombre.requerido` | Dominio | 422 |
| `proveedor.nombre.caracteres_invalidos` | Dominio | 422 |
| `proveedor.nombre.duplicado` | Aplicación (validación previa) | 409 |
| `proveedor.eliminado` | Dominio (editar un eliminado) | 422 |
| `integridad.duplicado` | Infraestructura (si la validación previa se saltó por concurrencia) | 409 |

## Pruebas

- Unitarias: `Entidades/ProveedorTests.cs` (dominio), `Aplicacion/ProveedorAppServiceTests.cs` (orquestación con repositorio simulado).
- Integración: `Persistencia/IndicesUnicosTests.cs` (índice único contra PostgreSQL real).
- Verificación manual end-to-end en esta iteración: alta, duplicado rechazado (`409`), caracteres inválidos rechazados (`422`) y listado, probados contra PostgreSQL real vía MVC (formulario con antiforgery token) y vía API (`curl`), con Swagger accesible en `/swagger`.
