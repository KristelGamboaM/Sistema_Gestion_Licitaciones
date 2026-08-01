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