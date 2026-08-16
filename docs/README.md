# Documentación del Sistema de Gestión de Licitaciones

Este archivo es el índice de navegación de toda la documentación del proyecto (sustituye al README tradicional de la raíz, spec §15.2). Toda la documentación vive en `/docs`, en Markdown; no hay documentos Word/PDF/PowerPoint ni enlaces externos.

## Empezar aquí

- [vision-alcance.md](vision-alcance.md): qué resuelve el sistema y qué queda fuera de alcance.
- [historias-usuario.md](historias-usuario.md): 18 historias de usuario con prioridad, estimación y criterios de aceptación.
- [plan-xp.md](plan-xp.md), con los roles, el Planning Game, el plan de liberación por iteración y las reglas de trabajo del equipo.
- [bitacora-xp.md](bitacora-xp.md), el registro real de las 9 iteraciones ejecutadas (resultado, retroalimentación y pendientes de cada una).

## Arquitectura y datos

- [arquitectura-general.md](arquitectura-general.md): monolito modular, capas y principios aplicados (con diagrama).
- [modelo-datos.md](modelo-datos.md): diagrama entidad-relación y convenciones de columnas.
- [integracion-modulos.md](integracion-modulos.md), con cómo cooperan los módulos, el diagrama de secuencia del flujo oferta→aprobación y la trazabilidad contra el flujo funcional mínimo del enunciado.

## Módulos

| Módulo | Documento |
| --- | --- |
| Persistencia (EF Core + PostgreSQL) | [Modulos/persistencia.md](Modulos/persistencia.md) |
| Proveedores | [Modulos/proveedores.md](Modulos/proveedores.md) |
| Licitaciones | [Modulos/licitaciones.md](Modulos/licitaciones.md) |
| Ofertas | [Modulos/ofertas.md](Modulos/ofertas.md) |
| Niveles de aprobación | [Modulos/niveles-aprobacion.md](Modulos/niveles-aprobacion.md) |
| Tipo de cambio | [Modulos/tipo-cambio.md](Modulos/tipo-cambio.md) |
| Interfaz Web | [Modulos/interfaz-web.md](Modulos/interfaz-web.md) |
| API REST | [Modulos/api-rest.md](Modulos/api-rest.md) |

## API, pruebas y despliegue

- [api.md](api.md): endpoints, contratos, `ProblemDetails` y ejemplos. Colección reproducible: [api-requests.http](api-requests.http).
- [pruebas.md](pruebas.md), con la estrategia de pruebas (unitarias, integración, funcionales E2E), la cobertura (93.2 %) y los tres bugs reales que las pruebas encontraron.
- [docker.md](docker.md): Dockerfile, Compose y verificación real de persistencia.
- [kubernetes.md](kubernetes.md): los 10 manifiestos, validación con `kubeconform` y cómo desplegar con un clúster disponible.
- [uso-ia.md](uso-ia.md), la declaración de uso de herramientas de IA (spec §16).

## Cómo levantar el proyecto localmente

```bash
dotnet restore Licitaciones.sln
dotnet build Licitaciones.sln
dotnet test tests/Licitaciones.UnitTests/Licitaciones.UnitTests.csproj
```

Para la aplicación completa con PostgreSQL real, ver [docker.md](docker.md) (`docker compose up --build`) o [persistencia.md](Modulos/persistencia.md) para levantar solo la base de datos en desarrollo local.
