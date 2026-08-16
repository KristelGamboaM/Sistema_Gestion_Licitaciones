# Módulo: Tipo de Cambio

## Propósito

Administrar tasas CRC/USD y exponer la conversión referencial que usa la interfaz para alternar montos entre colones y dólares, sin depender de Internet.

## Responsabilidades por capa

- **Domain** (`Entidades/TipoCambio.cs`): tasa > 0, `Activar`/`Desactivar`, `ConvertirCrcAUsd` (redondeo a 2 decimales).
- **Application** (`TiposCambio/TipoCambioAppService.cs`): `ActivarAsync` desactiva el tipo de cambio previamente activo antes de activar el nuevo (una sola transacción vía `IUnitOfWork`); `ConvertirCrcAUsdAsync` resuelve el activo y calcula el monto en USD sin persistirlo.
- **Web / Api**: CRUD + acción de activar; endpoint de conversión (`GET /api/v1/tipos-cambio/convertir?montoCRC=...`) reutilizado por el toggle CRC/USD global (ver [interfaz-web.md](interfaz-web.md)).

## Reglas de negocio (US-11, US-12)

1. Los valores oficiales se almacenan únicamente en CRC (`numeric(18,2)` en el resto del sistema); el USD nunca se persiste, es siempre calculado.
2. Solo puede existir un tipo de cambio activo, reforzado también por un índice único parcial en PostgreSQL (`WHERE "Activo" = true`, ver [persistencia.md](persistencia.md)) como defensa en profundidad.
3. El sistema funciona sin Internet: la tasa activa se administra localmente, no hay llamada a ninguna API externa de cambio.
4. Se muestra siempre la fecha de vigencia de la tasa usada.

## Errores

| Código | Situación | HTTP |
| --- | --- | --- |
| `tipo_cambio.tasa.invalida` | Tasa ≤ 0 | 422 |
| `tipo_cambio.sin_activo` | Se solicita conversión sin ningún tipo de cambio activo | 422 |

## Datos semilla

Un tipo de cambio inicial activo (₡520.00 por USD) se carga por migración; ver [persistencia.md](persistencia.md).

## Pruebas

- Unitarias: `Entidades/TipoCambioTests.cs` (tasa inválida, conversión, no altera el CRC almacenado), `Aplicacion/TipoCambioAppServiceTests.cs` (activar desactiva el anterior, conversión sin activo lanza error controlado).
