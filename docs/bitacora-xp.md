## Iteración 0 — Preparación
**Fecha:** 31 de julio 2026
**Objetivo:** Crear la estructura de la solución .NET (5 proyectos + 3 de pruebas)
y conectar las referencias entre capas.
**Resultado:**
- Solución compilable con `dotnet build` (build succeeded).
- Proyectos creados: Domain, Application, Infrastructure, Web, Api, UnitTests,
  IntegrationTests, FunctionalTests.
- Referencias conectadas: Application → Domain, Infrastructure → Application,
  Web → Infrastructure, Api → Infrastructure, UnitTests → Domain/Application.
**Pendiente:** Planning Game de la iteración 1 (validar historias US-01/US-02,
prioridad, estimación, criterios de aceptación).

## Iteración 1 — Planning Game
**Objetivo:** Redactar visión y alcance, historias de usuario con criterios de
aceptación verificables, y el plan XP (roles, plan de liberación, reglas de
trabajo del equipo). Corregir higiene del repositorio.
**Resultado:**
- `docs/vision-alcance.md`, `docs/historias-usuario.md` (18 historias, US-01 a
  US-18) y `docs/plan-xp.md` completos.
- Se detectó que `bin/`/`obj/` estaban versionados desde la Iteración 0
  (568 archivos); se agregó `.gitignore` y se retiraron del control de
  versiones.
- Se eliminaron los `Class1.cs` de plantilla en Domain/Application/Infrastructure.
- `dotnet build Licitaciones.sln`: compilación correcta, 0 advertencias.
**Retroalimentación del Cliente:** las historias deben quedar agrupadas por
módulo y enlazadas a la iteración donde se implementan, para poder verificar
trazabilidad al final del proyecto. Se incorporó en la columna "Trazabilidad"
de cada historia.
**Pendiente:** Iteración 2: dominio, reglas de negocio puras (TDD) y modelo
de persistencia con PostgreSQL.

## Iteración 2 — Dominio y Persistencia
**Objetivo:** Modelar las cinco entidades y sus reglas de negocio puras con
TDD (rojo→verde→refactor), y construir la capa de persistencia con EF Core 9
sobre PostgreSQL real.
**Resultado (ciclo TDD):**
- Se escribieron primero las pruebas (`ProveedorTests`, `LicitacionTests`,
  `OfertaTests`, `NivelAprobacionTests`, `TipoCambioTests`,
  `CalculadoraMejorOfertaTests`, `ResolutorNivelAprobacionTests`,
  `NormalizacionTextoTests`) y luego la implementación mínima en
  `Licitaciones.Domain` hasta ponerlas en verde; 75/75 pruebas unitarias
  pasan (`dotnet test tests/Licitaciones.UnitTests`).
- Entidades: `Licitacion` (máquina de estados Borrador/Publicada/Cerrada,
  cierre efectivo por fecha vencida), `Proveedor` (normalización Unicode y
  unicidad), `Oferta`, `NivelAprobacion` (rangos no traslapados), `TipoCambio`.
  `IReloj` inyectable para pruebas deterministas.
- Persistencia (`Licitaciones.Infrastructure`): `LicitacionesDbContext`,
  configuraciones EF Core, migración inicial (`InicialLicitaciones`),
  repositorios, `UnitOfWork` con traducción de errores de PostgreSQL a
  `ReglaNegocioException`, datos semilla (3 niveles de aprobación, 1 tipo de
  cambio activo).
- Se levantó PostgreSQL 16 en Docker, se aplicó la migración (`dotnet ef
  database update`) y se inspeccionó el esquema resultante (tablas, índices
  únicos, llaves foráneas) directamente con `psql`. Se agregaron 8 pruebas de
  integración (`tests/Licitaciones.IntegrationTests/Persistencia`) contra
  PostgreSQL real vía Testcontainers: migraciones, semilla, violación de los
  tres índices únicos y conflicto de concurrencia optimista (`xmin`), con las
  8 en verde.
- `UseXminAsConcurrencyToken()` (documentado en versiones antiguas del
  proveedor Npgsql) no existe en `Npgsql.EntityFrameworkCore.PostgreSQL`
  9.0.4; se reemplazó por el patrón portable de EF Core (propiedad sombra
  mapeada a `xmin`). Documentado en [persistencia.md](Modulos/persistencia.md).
- Se detectaron conflictos de versión de Entity Framework Core entre
  proyectos (advertencia MSB3277) al conectar `Infrastructure` con
  `Web`/`Api`. Se resolvió con Central Package Management
  (`Directory.Packages.props`); `dotnet build Licitaciones.sln` queda en 0
  advertencias, 0 errores.
- `docs/modelo-datos.md`, `docs/arquitectura-general.md` y
  `docs/Modulos/persistencia.md` completos con diagramas Mermaid.
**Retroalimentación del Cliente:** aprobar solo con evidencia contra
PostgreSQL real (no SQLite ni InMemory); se incorporó desde el inicio de esta
iteración.
**Pendiente:** Iteración 3, con los módulos Proveedores y Licitaciones (CRUD
+ capa de aplicación + MVC + API).

## Iteración 3 — Proveedores y Licitaciones
**Objetivo:** Construir el flujo completo (Application + MVC + API) de los
dos primeros módulos CRUD, con verificación real de extremo a extremo (no
solo pruebas automatizadas).
**Resultado:**
- **Proveedores:** `ProveedorAppService`, `ProveedoresController` (MVC, con
  búsqueda/paginación/alta/edición/eliminación con confirmación) y
  `ProveedoresController` (API REST `/api/v1/proveedores`). Manejador global
  de excepciones (`ExcepcionesDeNegocioHandler`) que traduce
  `ReglaNegocioException`/`EntidadNoEncontradaException` a `ProblemDetails`
  con código, correlación y el estado HTTP correcto. Swagger habilitado en
  `/swagger`.
- **Licitaciones:** `LicitacionAppService` (crear, editar, publicar, cerrar,
  eliminar, listar, mejor oferta con nombre de proveedor y aprobador
  resueltos), controladores MVC y API equivalentes, captura de fecha/hora de
  cierre con `type="datetime-local"`.
- Se levantó PostgreSQL 16 en Docker y se ejecutaron Web y Api reales; por
  `curl` se probaron alta, duplicados (`409`), caracteres inválidos (`422`),
  publicación de licitación, "Sin ofertas válidas" y el formulario MVC
  completo con token antifalsificación.
- Npgsql exige escribir `timestamptz` con `DateTimeOffset` en UTC (offset 0).
  Capturar la fecha de cierre en hora de Costa Rica (-06:00) sin normalizar
  causaba un `500`, y se corrigió en dos niveles: `ZonaHorariaCostaRica`
  interpreta el formulario como hora de Costa Rica y la normaliza a UTC, y
  `LicitacionesDbContext` aplica una conversión global a todo
  `DateTimeOffset` como defensa adicional para cualquier origen (incluida la
  API). También se detectó que los enums no se aceptaban como texto en JSON;
  se agregó `JsonStringEnumConverter` a la Api.
- Se fusionaron `ActualizarDatosBorrador` y `ActualizarPresupuesto` en
  `Licitacion` (duplicaban la regla de "no bajar presupuesto por debajo de
  una oferta existente") tras notar que solo se edita en Borrador, donde
  nunca hay ofertas. La refactorización no alteró el comportamiento
  observable; las pruebas quedaron actualizadas y en verde.
- 87/87 pruebas unitarias en verde tras agregar `ProveedorAppServiceTests` y
  `LicitacionAppServiceTests`.
- `docs/Modulos/proveedores.md` y `docs/Modulos/licitaciones.md` completos.
**Retroalimentación del Cliente:** verificar siempre contra un servidor real
levantado, no solo pruebas automatizadas. Quedó como práctica para el
resto de módulos.
**Pendiente:** Iteración 4: módulos Ofertas, Niveles de Aprobación y Tipo de
Cambio.

## Iteración 4 — Ofertas, Niveles de Aprobación y Tipo de Cambio
**Objetivo:** Completar los tres módulos CRUD restantes y la regla de
conversión monetaria, cerrando así todo el backend funcional del enunciado.
**Resultado:**
- **Ofertas:** se agregó `Oferta.ActualizarMonto` al dominio (TDD: prueba
  primero) porque el CRUD del enunciado exige edición y solo existía
  `Registrar`. `OfertaAppService` valida licitación publicada y no vencida,
  no duplicidad por proveedor, monto ≤ presupuesto y proveedor existente.
- **Niveles de Aprobación:** `NivelAprobacionAppService` valida ausencia de
  traslape (reutilizando `NivelAprobacion.SeTraslapaCon` del dominio) tanto
  al crear como al editar, excluyendo el propio registro en edición.
- **Tipo de Cambio:** `TipoCambioAppService` con activación exclusiva y
  conversión CRC→USD.
- PostgreSQL 16 en Docker + Api real. Se probaron: oferta válida, duplicada
  (`409`), sobre presupuesto (`422`), y (avanzando el reloj real 7 segundos
  sobre una licitación con cierre inminente) el rechazo de una oferta
  vencida (`409`); traslape de niveles de aprobación (`409`) contra la
  semilla; conversión CRC/USD.
- Activar un tipo de cambio mientras otro estaba activo fallaba con
  `integridad.duplicado`. La causa: el índice único parcial de PostgreSQL
  (`WHERE "Activo" = true`) no admite verificación diferida, y EF Core no
  garantiza que el `UPDATE` de desactivación se ejecute antes que el de
  activación dentro del mismo `SaveChanges`. Se corrigió separando
  desactivación y activación en dos llamadas a `GuardarCambiosAsync`. Se
  agregó una prueba de integración de regresión
  (`TipoCambioAppServiceIntegrationTests`) que solo puede reproducirse contra
  PostgreSQL real, no con datos en memoria.
- 107/107 pruebas unitarias y 9/9 de integración en verde.
- `docs/Modulos/ofertas.md`, `docs/Modulos/niveles-aprobacion.md` y
  `docs/Modulos/tipo-cambio.md` completos.
**Retroalimentación del Cliente:** cuando una regla de negocio dependa de un
efecto colateral en PostgreSQL (constraints, índices parciales), agregar una
prueba de integración específica además de la unitaria: no todo bug se
detecta con dobles de prueba en memoria.
**Pendiente:** Iteración 5, con landing page, navegación completa entre los
cinco módulos, modo claro/oscuro y toggle CRC/USD global.

## Iteración 5 — Landing page y experiencia de usuario global
**Objetivo:** Unificar la navegación entre los cinco módulos, construir la
landing page explicativa y agregar modo claro/oscuro y el alternador CRC/USD
que faltaban (§5.1 y §9 del enunciado).
**Resultado:**
- `Views/Home/Index.cshtml`: landing con el flujo publicar → ofertar →
  aprobar y enlaces a los cinco módulos y a Swagger; usa clases responsive de
  Bootstrap 5.3 (ya vendorizado, sin CDN).
- `_Layout.cshtml` unificado: menú con los seis destinos requeridos, botones
  "🌓 Tema" y "Ver en $ USD" en la barra superior, alertas de éxito/error
  reutilizables desde `TempData` en todas las páginas.
- Modo claro/oscuro con `data-bs-theme` (soporte nativo de Bootstrap 5.3),
  persistido en `localStorage`, respetando `prefers-color-scheme` como
  valor inicial.
- Toggle CRC/USD sin recargar la página: `_Layout.cshtml` resuelve el tipo de
  cambio activo una sola vez por solicitud (`ITipoCambioAppService`) y lo
  expone en atributos de `<body>`; `site.js` reformatea cada elemento
  `[data-monto-crc]` con `Intl.NumberFormat`. Se marcaron los montos en
  Licitaciones, Ofertas y Niveles de Aprobación.
- Se eliminó la vista `Privacy` heredada de la plantilla (código muerto sin
  relación con el enunciado).
- Verificación real: se sirvió la landing y las cinco páginas de listado
  contra PostgreSQL real, confirmando por `curl` los atributos
  `data-crc-por-usd`, `data-monto-crc` y la presencia de ambos botones;
  `node -c` validó la sintaxis de `site.js`. 107/107 pruebas unitarias siguen
  en verde.
- `docs/Modulos/interfaz-web.md` completo.
**Retroalimentación del Cliente:** el toggle de moneda debe funcionar sin
recargar la página y sin depender de que la API esté corriendo por separado.
Se resolvió haciendo que Web resuelva el tipo de cambio activo directamente
contra la base de datos, no contra la Api.
**Pendiente:** Iteración 6, documentar la integración entre todos los
módulos (`integracion-modulos.md`) y completar la suite de pruebas
(unitarias, integración y funcionales E2E con Playwright) con cobertura.

## Iteración 6 — Integración de módulos y suite de pruebas completa
**Objetivo:** Documentar cómo cooperan los módulos de extremo a extremo y
completar la pirámide de pruebas exigida (§12): unitarias, integración con
PostgreSQL real y funcionales E2E con Playwright, alcanzando la cobertura
mínima (§12.4).
**Resultado:**
- `docs/integracion-modulos.md` con diagrama de dependencias entre módulos,
  diagrama de secuencia del flujo oferta→mejor oferta→aprobación, y la
  tabla de trazabilidad del flujo funcional mínimo (§5.3) contra los
  documentos de cada módulo.
- `docs/api.md` y `docs/api-requests.http`: colección reproducible de
  solicitudes (crear proveedor → licitación → publicar → ofertar →
  duplicada 409 → sobre presupuesto 422 → mejor oferta → cerrar).
- **Pruebas funcionales E2E con Playwright** (`Licitaciones.FunctionalTests`):
  arrancan PostgreSQL real (Testcontainers) y `Licitaciones.Web` como
  proceso real (`dotnet Licitaciones.Web.dll`, no `WebApplicationFactory` en
  memoria; se abandonó ese enfoque tras confirmar que `TestServer` no es
  navegable por un navegador real), y Chromium headless. 9 pruebas cubren
  landing/navegación, modo claro/oscuro, CRUD de proveedores, flujo completo
  licitación→publicar→ofertar→mejor oferta, rechazo de duplicada/sobre
  presupuesto, y el toggle CRC/USD.
- Dos bugs reales, encontrados y corregidos por las pruebas funcionales, que
  no habrían aparecido en pruebas unitarias ni de integración:
  1. La regex `^[\p{L}\p{N} .,()]+$` del formulario de proveedor es válida
     en .NET pero jQuery Validate la traduce a un `RegExp` de JavaScript sin
     el flag `u`, donde `\p{L}` no es una clase Unicode, y bloqueaba
     silenciosamente el registro de cualquier proveedor. Se corrigió con un
     patrón equivalente compatible con JavaScript (`À-ÖØ-öø-ÿ`), dejando la
     regla completa (`\p{L}\p{N}`, Unicode total) como autoridad en el
     dominio.
  2. El formulario de ofertas pasaba una tupla con nombres
     (`(Guid Id, string Nombre)`) a `SelectList`, que resuelve "Id"/"Nombre"
     por reflexión. Una tupla solo expone `Item1`/`Item2` en tiempo de
     ejecución, lo que causaba un `NullReferenceException` al abrir el
     formulario. Se corrigió con un `record ProveedorOpcion(Guid Id, string
     Nombre)`.
- Se agregaron `RepositoriosTests` de integración (antes los repositorios
  concretos tenían 0 % de cobertura porque las pruebas existentes usaban el
  `DbContext` directamente) y pruebas unitarias adicionales de aplicación
  para `ActualizarAsync`/`EliminarAsync`/`ListarAsync` en los cinco
  servicios.
- **Cobertura final** (`coverlet` + `reportgenerator`, unitarias +
  integración): Domain 92.3 %, Application 90.1 %, Infrastructure 94.9 %,
  total del proyecto 93.2 %, que supera las metas de 80 %/70 % (§12.4).
- 124 pruebas unitarias + 13 de integración + 9 funcionales = 146 pruebas,
  todas en verde.
- `docs/pruebas.md` completo, con los tres bugs reales documentados junto a
  las cifras de cobertura.
**Retroalimentación del Cliente:** las pruebas funcionales deben ejecutar la
aplicación real (Kestrel), no un servidor en memoria: un bug de validación
de cliente nunca se habría detectado contra `TestServer`.
**Pendiente:** Iteración 7: Docker (Dockerfile, Compose) con verificación
real de `docker compose up --build`.

## Iteración 7 — Docker
**Objetivo:** Contenerizar la solución completa (Web, Api, migraciones y
PostgreSQL) y verificar `docker compose up --build` de extremo a extremo,
incluida la persistencia tras reiniciar contenedores (§13.1).
**Resultado:**
- Se agregó `Licitaciones.Migrator`, un proyecto de consola mínimo que
  aplica las migraciones pendientes y termina, de modo que la migración sea
  un paso controlado y separado del arranque de Web/Api (spec §13.2) en vez
  de algo implícito en cada inicio del contenedor.
- Un único `Dockerfile` multi-stage (SDK → runtime `aspnet:9.0`,
  usuario no privilegiado `appuser`) parametrizado con
  `--build-arg PROJECT=<Web|Api|Migrator>`, en vez de tres Dockerfiles
  casi idénticos.
- `docker-compose.yml`: `db` (con healthcheck y volumen), `migrate`
  (espera a que `db` esté saludable, corre una vez), `web` y `api` (esperan
  a que `migrate` termine con éxito antes de arrancar).
- Se verificó la ejecución real, no solo el archivo: `docker compose build` (las
  tres imágenes compilan), `docker compose up -d` (los cuatro contenedores
  llegan a `healthy`/`Exited (0)`), landing page y Swagger responden `200`,
  creación de un proveedor por API (`201`) y, tras `docker compose down`
  (sin `-v`) seguido de `up -d`, **el proveedor seguía presente**: persistencia
  confirmada, no asumida.
- `docs/docker.md` completo con la evidencia de esta verificación.
**Retroalimentación del Cliente:** las migraciones no deben aplicarse
implícitamente al arrancar Web/Api; deben ser un paso explícito y
verificable por separado. Se incorporó con el servicio `migrate`.
**Pendiente:** Iteración 8: Kubernetes (manifiestos completos) y GitHub
Actions (integración continua).

## Iteración 8 — Kubernetes e Integración Continua
**Objetivo:** Completar los manifiestos de Kubernetes exigidos por el
enunciado (§13.2) y el flujo de GitHub Actions (§13.3).
**Resultado:**
- Los 8 manifiestos requeridos (`namespace`, `app-configmap`,
  `app-secret.example`, `postgres-pvc`, `postgres-statefulset`,
  `postgres-service`, `app-deployment`, `app-service`) más dos adicionales
  no exigidos por nombre pero necesarios para un sistema completo
  (`api-deployment.yaml`, `api-service.yaml`, para que la API también quede
  desplegada). El `initContainer` "migrate" en cada Deployment aplica las
  migraciones de forma controlada antes de servir tráfico, reutilizando la
  imagen `Licitaciones.Migrator` de la iteración anterior.
- `k8s/app-secret.example.yaml` es una plantilla explícita (placeholder
  literal, no una credencial ofuscada); el archivo real
  (`k8s/app-secret.yaml`) está en `.gitignore`.
- **Sin clúster local disponible** (confirmado con la persona usuaria antes
  de empezar el proyecto): no se hizo un despliegue en vivo. En su lugar se
  validó lo que sí era verificable en esta máquina:
  - Las 10 definiciones YAML son válidas contra el **esquema real de
    Kubernetes 1.29** con `kubeconform` (`docker run
    ghcr.io/yannh/kubeconform`): `Valid: 10, Invalid: 0, Errors: 0`.
  - Las tres imágenes (`licitaciones-web`, `licitaciones-api`,
    `licitaciones-migrator`) se construyeron con los nombres exactos
    referenciados en los manifiestos.
  - `kubectl apply --dry-run=client` (v1.34.1) no funcionó sin clúster ni
    siquiera en modo cliente; se documentó como limitación conocida del
    binario instalado en vez de omitirlo silenciosamente.
- `.github/workflows/ci.yml`: restaurar, compilar con `-warnaserror`,
  `dotnet format --verify-no-changes`, revisión de dependencias vulnerables,
  pruebas unitarias + integración + funcionales (con instalación de
  navegadores de Playwright) con cobertura, publicación del reporte de
  cobertura como artefacto, construcción de las tres imágenes Docker, y
  validación de los manifiestos con `kubeconform`, todo en jobs separados
  para paralelizar.
- Los pasos críticos del CI también se probaron en esta máquina antes de
  confiarlos al pipeline: `dotnet build -warnaserror` (0 advertencias),
  `dotnet format --verify-no-changes` (sin cambios pendientes) y
  `dotnet list package --vulnerable --include-transitive` (sin
  vulnerabilidades) se ejecutaron localmente con éxito.
- `docs/kubernetes.md` completo, incluyendo instrucciones de despliegue para
  quien sí cuente con un clúster y cómo verificaría la persistencia de datos
  de forma análoga a la de Docker Compose.
**Retroalimentación del Cliente:** cuando no se pueda verificar algo (sin
clúster disponible), documentar explícitamente qué se verificó y qué no, en
vez de simplemente afirmar que "funciona". Se aplicó en toda esta iteración.
**Pendiente:** Iteración 9, documentación final restante y etiqueta de
entrega `v1.0.0`.

## Iteración 9 — Documentación final y cierre
**Objetivo:** Completar los últimos archivos de `/docs` pendientes y dejar
el repositorio listo para la etiqueta de entrega.
**Resultado:**
- `docs/uso-ia.md`: declaración honesta del uso de Claude Code, con
  herramienta, finalidad, módulos asistidos, ejemplos concretos (incluidos
  los tres bugs reales corregidos durante la propia sesión) y, explícitamente,
  la responsabilidad pendiente del equipo antes de la entrega y la defensa
  oral (leer y ejecutar cada módulo, modificar en vivo al menos un cambio por
  capa, y no usar "la IA lo generó" como explicación).
- `docs/Modulos/api-rest.md`: módulo de la capa API REST (antes solo
  documentada indirectamente en `docs/api.md`), completando los 8 documentos
  de `/docs/Modulos` exigidos por §15.1.
- `docs/README.md` reescrito como índice de navegación real de toda la
  documentación (antes solo tenía el comando de arranque local), conforme al
  spec §15.2: "sustituye el README documental tradicional ubicado en la raíz".
- Se detectó una inconsistencia entre `docs/plan-xp.md` (planificaba Docker
  y Kubernetes como una sola iteración 7) y `docs/bitacora-xp.md` (quedaron
  registradas como iteraciones 7 y 8 separadas); se corrigió la tabla de
  `plan-xp.md` para reflejar las nueve iteraciones reales en vez de ajustar
  la bitácora a un plan que ya no correspondía con lo ejecutado.
- Verificación final de cierre: `dotnet build Licitaciones.sln` (0
  advertencias) y las tres suites de pruebas: 124 unitarias + 13 de
  integración + 9 funcionales E2E = **146 pruebas, todas en verde**.
- Todos los archivos de `/docs` y `/docs/Modulos` exigidos por §15.1 tienen
  contenido real (ninguno queda vacío).
**Retroalimentación del Cliente:** la trazabilidad importa más que la
cantidad de documentos. Se prefirió corregir la inconsistencia entre plan y
bitácora en vez de dejarla, aunque ambos documentos ya existían.
**Cierre:** el repositorio queda listo para que el equipo revise, se
apropie del código (ver [uso-ia.md](uso-ia.md)) y etiquete la entrega final
como `v1.0.0`/`entrega-final` una vez completada esa revisión.

## Sesión de evidencia — Docker y Kubernetes en vivo
**Fecha:** 14 de agosto de 2026.
**Objetivo:** Capturar la evidencia fotográfica pendiente en `docs/assets/`
(§15.1) y completar el despliegue en vivo de Kubernetes que había quedado
sin verificar en la Iteración 8, siguiendo `scripts/checklist-capturas.md`
en una llamada por Discord con pantalla compartida.
**Programación en parejas:** rotación piloto/copiloto por bloque: Kristel
Gamboa piloto y Geiner Alfaro copiloto en el bloque de Docker Compose;
Geiner Alfaro piloto y Kristel Gamboa copiloto en el bloque de Kubernetes.
**Resultado:**
- **Docker Compose:** capturas de `docker compose ps` (cuatro servicios
  saludables), landing y Swagger servidos desde los contenedores, y la
  prueba de persistencia completa (proveedor creado → `docker compose
  down`/`up` sin `-v` → el proveedor seguía presente).
- **Kubernetes:** a diferencia de la Iteración 8 (sin clúster disponible),
  esta vez se levantó un clúster local con `kind`, se cargaron las tres
  imágenes (`licitaciones-web`, `licitaciones-api`, `licitaciones-migrator`)
  y se aplicaron los 10 manifiestos en el orden documentado en
  `kubernetes.md`. Capturas de los pods en `Running`/`Ready`, servicios,
  PVC `postgres-data` en `Bound`, logs del `initContainer` de migración, y
  la prueba de persistencia (proveedor creado → `kubectl delete pod
  postgres-0` → el StatefulSet lo recreó → el proveedor seguía existiendo).
- `docs/docker.md` y `docs/kubernetes.md` actualizados con las 11 capturas
  enlazadas; se corrigió la sección "Estado de esta verificación" de
  `kubernetes.md`, que ya no describe un despliegue sin verificar.
**Retroalimentación del Cliente:** —
**Pendiente:** revisar que ningún dato de prueba usado en las capturas
delate autoría de IA (spec §16), y proceder con la etiqueta de entrega
`v1.0.0`/`entrega-final`.