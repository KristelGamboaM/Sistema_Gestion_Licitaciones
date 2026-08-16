# Pruebas

## Estrategia

Tres niveles, cada uno con una responsabilidad distinta y sin solaparse innecesariamente:

| Nivel | Proyecto | Qué verifica | Contra qué corre |
| --- | --- | --- | --- |
| Unitarias | `tests/Licitaciones.UnitTests` | Reglas de negocio puras del dominio y orquestación de los servicios de aplicación (con repositorios simulados) | En memoria, sin PostgreSQL |
| Integración | `tests/Licitaciones.IntegrationTests` | Migraciones, índices únicos, llaves foráneas, concurrencia optimista y los repositorios reales | PostgreSQL 16 real (Testcontainers) |
| Funcionales E2E | `tests/Licitaciones.FunctionalTests` | El flujo funcional mínimo del enunciado (§5.3) desde un navegador real | Chromium (Playwright) + `Licitaciones.Web` real + PostgreSQL real (Testcontainers) |

No se usa SQLite ni una base de datos en memoria para las pruebas de integración o funcionales: la spec lo prohíbe explícitamente (§11) y, como se documenta abajo, dos bugs reales solo eran reproducibles contra el motor real.

## Ejecución

```bash
# Unitarias (rápidas, sin Docker)
dotnet test tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj

# Integración (requiere Docker corriendo)
dotnet test tests/Licitaciones.IntegrationTests/Licitaciones.IntegrationTests.csproj

# Funcionales E2E (requiere Docker + navegadores de Playwright instalados una vez)
pwsh tests/Licitaciones.FunctionalTests/bin/Debug/net9.0/playwright.ps1 install chromium
dotnet test tests/Licitaciones.FunctionalTests/Licitaciones.FunctionalTests.csproj
```

Las pruebas de integración y funcionales levantan su propio contenedor PostgreSQL 16 vía Testcontainers (biblioteca .NET, no requiere `docker-compose` corriendo de antemano) y lo destruyen al finalizar. Las funcionales, además, arrancan `Licitaciones.Web` como un proceso real (`dotnet Licitaciones.Web.dll`) en un puerto libre, y un Chromium headless para interactuar con la interfaz igual que lo haría una persona usuaria.

## Casos principales por módulo

Ver la sección "Pruebas" de cada [documento de módulo](Modulos/) para el detalle. En conjunto cubren:

- Normalización y unicidad de proveedor y licitación (incluyendo Unicode/espacios).
- Máquina de estados completa de licitación, incluyendo el cierre efectivo por fecha vencida.
- Las tres reglas de rechazo de ofertas (duplicada, sobre presupuesto, licitación cerrada/vencida), verificadas también avanzando un reloj real 7 segundos en una prueba de integración y no solo con el reloj falso de las unitarias.
- Mejor oferta, desempate y las cuatro clasificaciones de ahorro.
- Traslape de niveles de aprobación, incluyendo el caso de dos rangos abiertos.
- Activación exclusiva de tipo de cambio y conversión CRC/USD.
- Landing page, navegación, modo claro/oscuro y toggle CRC/USD desde el navegador.
- CRUD completo de los cinco módulos desde el navegador, incluyendo mensajes de validación en cliente y servidor.

## Cobertura

Medida con `coverlet` (`--collect:"XPlat Code Coverage"`) sobre unitarias + integración, combinada con `reportgenerator`:

| Proyecto | Cobertura de líneas | Meta (spec §12.4) |
| --- | --- | --- |
| `Licitaciones.Domain` | 92.3 % | ≥ 80 % |
| `Licitaciones.Application` | 90.1 % | ≥ 80 % |
| `Licitaciones.Infrastructure` | 94.9 % | — |
| **Total del proyecto** | **93.2 %** | ≥ 70 % |

> Estas cifras se tomaron de una corrida local y deben refrescarse antes de la entrega final. El pipeline de CI (`.github/workflows/ci.yml`, paso "Verificar umbral de cobertura") ahora falla automáticamente si Domain+Application bajan de 80 % o el total baja de 70 %, usando `scripts/verificar-cobertura.py` sobre el reporte combinado de `reportgenerator`; el artefacto `coverage-report` de cada ejecución es la fuente de verdad, no esta tabla.

Para reproducir:

```bash
dotnet test tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet test tests/Licitaciones.IntegrationTests/Licitaciones.IntegrationTests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator "-reports:TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/Report" "-reporttypes:TextSummary;Html;Cobertura"
python3 scripts/verificar-cobertura.py TestResults/Report/Cobertura.xml
```

## Bugs reales encontrados por las pruebas (no simulados)

La cobertura numérica no sustituye la calidad de los escenarios (spec §12.4); estos tres hallazgos, cada uno detectado por un nivel de prueba distinto, lo ilustran:

1. **Concurrencia en activación de tipo de cambio** (integración): el índice único parcial de PostgreSQL rechazaba la activación de un nuevo tipo de cambio porque EF Core no garantiza el orden de los `UPDATE` dentro de un mismo `SaveChanges`. Solo reproducible contra PostgreSQL real.
2. **Regex de validación de cliente rota** (funcional): `^[\p{L}\p{N} .,()]+$` es válida en .NET pero, sin el flag `u`, jQuery Validate la traduce a un `RegExp` de JavaScript donde `\p{L}` no es una clase Unicode, y bloqueaba silenciosamente cualquier nombre de proveedor. Solo reproducible con un navegador real ejecutando la validación de cliente.
3. **`SelectList` con tupla con nombres** (funcional): pasar `IEnumerable<(Guid Id, string Nombre)>` a `SelectList(items, "Id", "Nombre")` lanzaba `NullReferenceException` en tiempo de ejecución porque los nombres de una tupla no son propiedades reflectables (solo existen `Item1`/`Item2` en tiempo de ejecución). Solo se manifestaba al renderizar la vista real.
