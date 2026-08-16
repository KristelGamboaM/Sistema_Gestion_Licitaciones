# Plan XP — Sistema de Gestión de Licitaciones

## Equipo y roles

Proyecto en pareja: **Kristel Gamboa** y **Geiner Alfaro**.

| Rol XP | Persona(s) | Notas |
| --- | --- | --- |
| Cliente | Persona docente (representada por los criterios de aceptación de [historias-usuario.md](historias-usuario.md)) | Define prioridad y valida cada entrega. |
| Programadores | Kristel Gamboa, Geiner Alfaro | Ambas personas deben poder explicar y modificar cualquier módulo (propiedad colectiva). |
| Encargado de pruebas de aceptación | Ambas personas, en conjunto con el Cliente | Verifica los criterios de aceptación al cierre de cada iteración. |

No se utilizan roles de Scrum (Product Owner, Scrum Master) ni artefactos de Kanban. El proceso se rige únicamente por Extreme Programming: Planning Game, historias de usuario, iteraciones cortas, TDD, integración continua, diseño simple, refactorización, programación en parejas y propiedad colectiva.

## Programación en parejas

Se trabaja con **rotación de roles piloto/copiloto** dentro de cada módulo: quien escribe la prueba en rojo cede el teclado a su compañero/a para la implementación mínima en verde, y ambas personas participan en la refactorización posterior. La alternancia se registra en [bitacora-xp.md](bitacora-xp.md) por iteración. Debido a que gran parte de la construcción inicial fue asistida por IA en una sesión intensiva (ver [uso-ia.md](uso-ia.md)), ambas personas son responsables de revisar, ejecutar, entender y poder defender cada línea de código entregado. La asistencia de IA no sustituye la programación en pareja ni la comprensión del equipo.

## Planning Game

1. El Cliente (criterios del enunciado oficial) plantea las necesidades funcionales.
2. El equipo redacta historias de usuario cortas y verificables ([historias-usuario.md](historias-usuario.md)).
3. El equipo estima cada historia en días ideales de programación en pareja.
4. El Cliente prioriza (Alta/Media/Baja).
5. El equipo agrupa historias en iteraciones de alcance similar, respetando dependencias técnicas (el dominio y la persistencia deben preceder a los módulos CRUD; los módulos CRUD deben preceder a la interfaz MVC y a la API).

## Plan de liberación (release plan)

Una única release final (`v1.0.0` / `entrega-final`) compuesta por releases pequeños y demostrables al cierre de cada iteración, según exige XP. Cada iteración deja el sistema en estado ejecutable (`dotnet build` y `dotnet test` en verde).

| Iteración | Alcance | Historias | Entregas del §14.1 del enunciado |
| --- | --- | --- | --- |
| 0 | Preparación: estructura de solución .NET, referencias entre capas | — | 1 (parcial) |
| 1 | Planning Game: historias de usuario y plan XP; limpieza de plantillas y `.gitignore` | — | 1, 2 |
| 2 | Dominio (entidades, reglas puras, `IReloj`) y persistencia (EF Core, PostgreSQL, migraciones, seed) | US-17 | 3, 4 |
| 3 | Módulos Proveedores y Licitaciones (CRUD + máquina de estados) | US-01, US-02, US-03, US-04, US-05 | 5, 6 |
| 4 | Módulos Ofertas, Niveles de Aprobación y Tipo de Cambio | US-06 a US-11 | 7, 8, 9 |
| 5 | Interfaz MVC (landing, navegación, temas, toggle CRC/USD) y API REST | US-12 a US-16, US-18 | 10, 11 |
| 6 | Integración entre módulos y suite de pruebas completa (unitarias, integración, funcionales E2E) con cobertura | — (verifica todas) | 12 |
| 7 | Docker (Dockerfile, Compose) con verificación real de persistencia | — | 13 |
| 8 | Kubernetes (manifiestos completos) e integración continua (GitHub Actions) | — | 14 |
| 9 | Documentación final y cierre | — | 15 |

Se cumplen las **al menos cuatro iteraciones** exigidas (hay nueve, además de la Iteración 0 de preparación), cada una con una versión ejecutable y demostrable al cierre.

## Reglas de trabajo XP del equipo

- **TDD:** ninguna regla de negocio se implementa sin una prueba que primero falle (rojo), luego el mínimo código para pasarla (verde) y finalmente refactorización. Esto aplica especialmente a las reglas del §8 del enunciado (unicidad, vencimiento, montos, mejor oferta, niveles de aprobación, conversión).
- **Integración continua:** cada cambio funcional se compila y se prueba antes de integrarse; a partir de la Iteración 8 esto lo automatiza GitHub Actions y bloquea la integración si el flujo falla.
- **Diseño simple:** se implementa la solución más sencilla que satisface las historias vigentes; no se anticipan abstracciones para requisitos no solicitados (por ejemplo, no se construyen microservicios: el enunciado permite monolito modular y aquí no está justificada la separación).
- **Refactorización constante:** se refactoriza sin alterar comportamiento observable; se documenta en la bitácora cuando una refactorización es significativa.
- **Propiedad colectiva:** cualquiera de las dos personas puede y debe poder modificar cualquier módulo.
- **Ritmo sostenible:** el trabajo se distribuye por iteración según la tabla anterior; la bitácora evidencia el avance por iteración en vez de una construcción concentrada al final.
- **Estándares de código:** nombres descriptivos en español para el dominio del negocio (Licitación, Proveedor, Oferta) y en inglés para infraestructura técnica genérica cuando corresponda; comentarios solo cuando una regla no es evidente por sí misma; sin código muerto ni mensajes de depuración.

## Definición de terminado (DoD) por historia

Una historia se considera terminada cuando:

1. Existe al menos una prueba automatizada por cada criterio de aceptación.
2. `dotnet build` compila sin advertencias evitables y `dotnet test` pasa en verde.
3. La funcionalidad es demostrable desde la interfaz MVC y/o la API, según corresponda.
4. La bitácora de la iteración registra el resultado, la retroalimentación y cualquier ajuste para la siguiente iteración.
