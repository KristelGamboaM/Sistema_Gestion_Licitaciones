# Historias de Usuario

Redactadas desde la perspectiva de la persona Cliente (representada por la persona docente/usuaria del sistema) durante el Planning Game. Cada historia incluye prioridad, estimación en días ideales de programación en pareja y criterios de aceptación verificables mediante pruebas automatizadas. La columna **Trazabilidad** enlaza cada historia con su iteración y con los archivos de prueba donde se demuestra su cumplimiento (ver [plan-xp.md](plan-xp.md) y [bitacora-xp.md](bitacora-xp.md)).

Escala de prioridad: **Alta** (bloquea otras historias o es núcleo del negocio), **Media** (valor funcional directo), **Baja** (mejora de experiencia, no bloqueante).

## Proveedores

### US-01 — Registrar proveedor
**Como** persona encargada de compras, **quiero** registrar un proveedor con un nombre válido y único **para** poder asociarle ofertas en las licitaciones.

- **Prioridad:** Alta
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. El nombre se normaliza (trim, colapso de espacios repetidos, normalización Unicode NFKC) antes de comparar unicidad.
  2. Dos nombres que difieren solo en mayúsculas/minúsculas o espacios se consideran duplicados y el segundo registro se rechaza con un mensaje controlado.
  3. Solo se permiten letras, números, espacios, punto, coma y paréntesis (`^[\p{L}\p{N} .,()]+$`); cualquier otro carácter se rechaza en cliente, servidor y base de datos.
  4. El identificador se genera automáticamente y no es editable.
- **Trazabilidad:** Iteración 3 (módulo Proveedores). Pruebas: `ProveedorTests` (unit), `ProveedorRepositoryTests` (integración), `crud_proveedores.spec` (funcional).

### US-02 — Editar y eliminar proveedor
**Como** persona encargada de compras, **quiero** editar o eliminar un proveedor **para** mantener el catálogo correcto sin perder el historial de ofertas ya registradas.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. Editar reaplica las mismas reglas de unicidad y caracteres permitidos que el alta.
  2. Un proveedor con ofertas relacionadas no se elimina físicamente; se aplica borrado lógico (`DeletedAt`).
  3. Se solicita confirmación antes de cualquier eliminación.
  4. Un proveedor eliminado lógicamente no aparece en los listados activos ni puede recibir nuevas ofertas.
- **Trazabilidad:** Iteración 3. Pruebas: `ProveedorServiceTests`, `ProveedorRepositoryTests`.

## Licitaciones

### US-03 — Crear licitación
**Como** persona encargada de compras, **quiero** crear una licitación con código único, título, presupuesto y fecha/hora de cierre **para** iniciar el proceso de recepción de ofertas.

- **Prioridad:** Alta
- **Estimación:** 3 días ideales
- **Criterios de aceptación:**
  1. El código se normaliza (trim + mayúsculas) para validar unicidad, ignorando diferencias de espacios y caso.
  2. El presupuesto debe ser mayor que cero; no se aceptan negativos ni cero.
  3. La fecha y hora de cierre se seleccionan mediante un control de calendario/hora (no solo texto libre) y se almacenan como `DateTimeOffset`.
  4. La licitación se crea en estado `Borrador`.
- **Trazabilidad:** Iteración 3 (módulo Licitaciones). Pruebas: `LicitacionTests`, `LicitacionRepositoryTests`, `crud_licitaciones.spec`.

### US-04 — Publicar licitación
**Como** persona encargada de compras, **quiero** publicar una licitación en `Borrador` **para** habilitar la recepción de ofertas.

- **Prioridad:** Alta
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. Solo se permite `Borrador → Publicada` cuando los datos están completos, el presupuesto es válido y la fecha de cierre es futura respecto al reloj inyectado.
  2. Cualquier otra transición (`Publicada → Borrador`, `Cerrada → *`) se rechaza con un mensaje controlado.
  3. Una licitación cuya `FechaCierre` ya pasó se considera **cerrada funcionalmente** en toda regla de negocio, aunque el campo `Estado` no se haya actualizado todavía.
- **Trazabilidad:** Iteración 3. Pruebas: `EstadoLicitacionTests` (cubre la máquina de estados completa con `IReloj` falso).

### US-05 — Cerrar o cancelar licitación
**Como** persona encargada de compras, **quiero** cerrar manualmente una licitación en `Borrador` o `Publicada` **para** documentar una cancelación o un cierre anticipado.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. `Borrador → Cerrada` y `Publicada → Cerrada` están permitidas como cierre/cancelación documentada.
  2. No existe transición de reapertura (`Cerrada → Publicada`/`Borrador`) salvo autorización expresa de la persona docente, no implementada en este alcance.
- **Trazabilidad:** Iteración 3. Pruebas: `EstadoLicitacionTests`.

## Ofertas

### US-06 — Registrar oferta válida
**Como** proveedor, **quiero** registrar una oferta económica sobre una licitación publicada **para** participar en el proceso.

- **Prioridad:** Alta
- **Estimación:** 3 días ideales
- **Criterios de aceptación:**
  1. Solo se aceptan ofertas sobre licitaciones `Publicada` y no cerradas funcionalmente.
  2. El monto ofertado debe ser mayor que cero y menor o igual al presupuesto (igual al presupuesto es válido).
  3. Un proveedor no puede registrar más de una oferta para la misma licitación (índice único compuesto `LicitacionId + ProveedorId`).
- **Trazabilidad:** Iteración 4 (módulo Ofertas). Pruebas: `OfertaTests`, `OfertaRepositoryTests`.

### US-07 — Rechazar oferta duplicada, vencida o sobre presupuesto
**Como** persona encargada de compras, **quiero** que el sistema rechace ofertas inválidas **para** garantizar la integridad del proceso de licitación.

- **Prioridad:** Alta
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. Una segunda oferta del mismo proveedor para la misma licitación se rechaza con mensaje controlado (409/conflicto).
  2. Una oferta con monto mayor al presupuesto se rechaza (igual está permitido).
  3. Una oferta registrada cuando la fecha/hora actual es igual o posterior a `FechaCierre` se rechaza, incluso si el campo `Estado` aún dice `Publicada`.
  4. Las ofertas de licitaciones cerradas no pueden editarse ni eliminarse; se conservan como evidencia.
- **Trazabilidad:** Iteración 4. Pruebas: `OfertaTests` (casos rojo→verde para cada rechazo).

### US-08 — Consultar mejor oferta y clasificación
**Como** persona encargada de compras, **quiero** ver la mejor oferta de una licitación, su porcentaje de ahorro y su clasificación **para** tomar la decisión de adjudicación.

- **Prioridad:** Alta
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. La mejor oferta es la de menor monto válido; en empate gana la registrada primero.
  2. Sin ofertas válidas se muestra "Sin ofertas válidas".
  3. Ahorro ≥ 10 % → "Oferta conveniente"; 0 % < ahorro < 10 % → "Oferta aceptable"; oferta = presupuesto → "Oferta válida sin ahorro".
  4. El porcentaje de ahorro se calcula como `((Presupuesto − MejorOferta) / Presupuesto) × 100`.
- **Trazabilidad:** Iteración 4. Pruebas: `MejorOfertaServiceTests` (todos los casos de clasificación).

## Niveles de aprobación

### US-09 — Administrar niveles de aprobación
**Como** persona administradora del sistema, **quiero** definir rangos de monto y su aprobador **para** que el sistema determine automáticamente quién aprueba cada licitación.

- **Prioridad:** Media
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. Los rangos no pueden traslaparse.
  2. Solo puede existir un rango abierto (sin monto máximo).
  3. El aprobador se resuelve consultando la tabla parametrizable (sin condicionales `if/else` fijos en el código).
- **Trazabilidad:** Iteración 4 (módulo Niveles de Aprobación). Pruebas: `NivelAprobacionServiceTests` (traslapes, rango abierto, búsqueda por monto).

### US-10 — Consultar aprobador de una licitación
**Como** persona encargada de compras, **quiero** ver qué nivel debe aprobar una licitación según su presupuesto **para** enrutarla correctamente.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. Dado un monto, se devuelve el único rango que lo contiene.
  2. Si ningún rango cubre el monto (configuración incompleta), se informa un mensaje controlado en vez de un error no manejado.
- **Trazabilidad:** Iteración 4. Pruebas: `NivelAprobacionServiceTests`.

## Tipo de cambio y conversión monetaria

### US-11 — Administrar tipos de cambio
**Como** persona administradora del sistema, **quiero** registrar tipos de cambio CRC/USD y marcar uno como activo **para** que la conversión referencial use siempre una tasa vigente y controlada localmente.

- **Prioridad:** Media
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. La tasa (`CRCporUSD`) debe ser mayor que cero.
  2. Solo un tipo de cambio puede estar `Activo = true` a la vez; activar uno nuevo desactiva el anterior.
  3. El sistema funciona sin conexión a Internet: la tasa activa se administra localmente, sin llamadas externas.
- **Trazabilidad:** Iteración 4 (módulo Tipo de Cambio). Pruebas: `TipoCambioServiceTests`.

### US-12 — Alternar montos entre CRC y USD
**Como** persona usuaria del sistema, **quiero** alternar visualmente los montos entre colones y dólares **para** interpretar los valores en la moneda que prefiera, sin alterar los datos originales.

- **Prioridad:** Media
- **Estimación:** 2 días ideales
- **Criterios de aceptación:**
  1. Los valores oficiales permanecen almacenados únicamente en CRC; USD es una representación calculada (`MontoCRC / CRCporUSD`).
  2. La interfaz muestra la fecha de vigencia del tipo de cambio utilizado.
  3. El formato de colones usa la cultura `es-CR`.
- **Trazabilidad:** Iteración 5 (MVC). Pruebas: `ConversionMonetariaTests` (unit), `toggle_moneda.spec` (funcional).

## Interfaz y experiencia

### US-13 — Landing page y navegación
**Como** persona visitante, **quiero** una página inicial que explique el propósito del sistema y un menú de navegación claro **para** entender el flujo completo antes de operar.

- **Prioridad:** Alta
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. La landing explica licitación, ofertas, mejor oferta, nivel de aprobación y conversión monetaria.
  2. El menú da acceso a Inicio, Licitaciones, Proveedores, Ofertas, Niveles de aprobación, Tipo de cambio y documentación interactiva de la API.
  3. El diseño es adaptable (responsive) a computadora y móvil.
- **Trazabilidad:** Iteración 5. Pruebas: `landing_navegacion.spec`.

### US-14 — Modo claro y modo oscuro
**Como** persona usuaria, **quiero** alternar entre modo claro y oscuro **para** ajustar la interfaz a mi preferencia visual.

- **Prioridad:** Baja
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. Existe un control visible para alternar el tema.
  2. La preferencia persiste entre visitas (almacenamiento local del navegador).
- **Trazabilidad:** Iteración 5. Pruebas: `modo_tema.spec`.

## API REST

### US-15 — Operar el sistema vía API REST
**Como** sistema externo o integrador, **quiero** ejecutar las mismas operaciones de negocio mediante una API REST versionada **para** integrarme sin depender de la interfaz web.

- **Prioridad:** Alta
- **Estimación:** 3 días ideales
- **Criterios de aceptación:**
  1. Los endpoints usan DTOs propios, nunca entidades de EF Core expuestas directamente.
  2. Las respuestas de error usan `ProblemDetails` con título, estado, detalle seguro, código de error e identificador de correlación, sin exponer stack traces ni rutas internas.
  3. Los listados soportan paginación, filtrado y ordenamiento.
  4. Los códigos HTTP corresponden a la operación (`200/201/204/400/404/409/422/500` según corresponda).
- **Trazabilidad:** Iteración 5 (API REST). Pruebas: `LicitacionesApiTests`, `OfertasApiTests` (integración con `WebApplicationFactory`).

### US-16 — Documentación interactiva de la API
**Como** persona desarrolladora integradora, **quiero** una documentación interactiva (OpenAPI/Swagger) **para** conocer los contratos sin leer el código fuente.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. La documentación está disponible en un endpoint accesible desde el menú de navegación.
  2. Cada endpoint documenta parámetros, cuerpo de solicitud, respuestas y códigos de error posibles.
- **Trazabilidad:** Iteración 5.

## Transversales

### US-17 — Concurrencia optimista
**Como** persona usuaria, **quiero** que el sistema detecte si otra persona modificó un registro antes que yo **para** evitar sobrescrituras silenciosas.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. Editar un registro modificado por otra transacción lanza `DbUpdateConcurrencyException`, capturada y traducida a un mensaje controlado.
  2. La detección usa la columna de versión de PostgreSQL (`xmin`), sin depender de comparar todos los campos manualmente.
- **Trazabilidad:** Iteración 2 (persistencia), verificado en cada módulo CRUD. Pruebas: `ConcurrenciaOptimistaTests` (integración, PostgreSQL real).

### US-18 — Mensajes de error controlados
**Como** persona usuaria, **quiero** recibir mensajes claros ante errores de validación o integridad **para** entender qué corregir sin ver detalles técnicos internos.

- **Prioridad:** Media
- **Estimación:** 1 día ideal
- **Criterios de aceptación:**
  1. Errores de validación se muestran junto al campo correspondiente en los formularios.
  2. Errores de integridad referencial (por ejemplo, eliminar un proveedor con ofertas) se traducen a un mensaje de negocio, no a la excepción cruda de PostgreSQL.
- **Trazabilidad:** Transversal a todos los módulos CRUD (iteraciones 3–5).
