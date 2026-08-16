# Módulo: Niveles de Aprobación

## Propósito

Mantener la tabla parametrizable de rangos de monto que determina automáticamente quién aprueba una licitación, sin condicionales fijos en el código.

## Responsabilidades por capa

- **Domain** (`Entidades/NivelAprobacion.cs`): invariantes de rango (mínimo > 0, máximo > mínimo cuando existe), `SeTraslapaCon` para comparar dos rangos. `Servicios/ResolutorNivelAprobacion.cs`: búsqueda del nivel que contiene un monto (`FirstOrDefault` sobre la tabla, no `if/else`).
- **Application** (`NivelesAprobacion/NivelAprobacionAppService.cs`): valida que el nuevo/editado rango no se traslape con ningún otro existente antes de guardar.
- **Web / Api**: CRUD estándar; `NivelesAprobacionController` en ambos.

## Reglas de negocio (US-09, US-10)

1. Los rangos no pueden traslaparse entre sí.
2. Solo puede existir un rango abierto (sin monto máximo); se detecta con la misma regla general de traslape, ya que dos rangos abiertos siempre se consideran traslapados.
3. El aprobador de un monto se resuelve consultando la tabla (`ResolutorNivelAprobacion.Resolver`), reutilizado por el módulo de Licitaciones al calcular la mejor oferta.

## Errores

| Código | Situación | HTTP |
| --- | --- | --- |
| `nivel.monto_minimo.invalido` | Monto mínimo ≤ 0 | 422 |
| `nivel.rango.invalido` | Monto máximo ≤ monto mínimo | 422 |
| `nivel.aprobador.requerido` | Aprobador vacío | 422 |
| `nivel.rango.traslape` | El rango se cruza con otro existente (incluye el caso de dos rangos abiertos) | 409 |

## Datos semilla

Los tres niveles del enunciado (§8.7) se cargan por migración; ver [persistencia.md](persistencia.md).

## Pruebas

- Unitarias: `Entidades/NivelAprobacionTests.cs` (rangos, traslape), `Servicios/ResolutorNivelAprobacionTests.cs` (resolución por monto), `Aplicacion/NivelAprobacionAppServiceTests.cs` (traslape contra repositorio simulado, incluyendo el caso de dos rangos abiertos, y que editar excluye el propio registro del chequeo).
