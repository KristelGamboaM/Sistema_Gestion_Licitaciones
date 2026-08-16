# Uso Responsable de Inteligencia Artificial

Declaración exigida por el enunciado (§16). Se describe honestamente cómo y para qué se usó IA, qué se verificó durante esa sesión, y qué queda como responsabilidad pendiente e ineludible del equipo (Kristel Gamboa y Geiner Alfaro).

## Herramienta

**Claude Code** (Anthropic), un agente de IA con acceso a terminal, sistema de archivos, Docker y control de versiones, operado de forma interactiva dentro del editor del equipo.

## Finalidad y alcance del uso

Ante el tamaño del proyecto (100 puntos, cinco módulos de negocio, persistencia real, interfaz Web, API REST, tres niveles de pruebas, contenedores, Kubernetes e integración continua) y una decisión explícita del equipo de construir una primera versión completa en una sesión intensiva asistida, se usó Claude Code para generar la **primera versión** de prácticamente todos los módulos: dominio, capa de aplicación, persistencia con EF Core/PostgreSQL, interfaz MVC, API REST, las tres suites de pruebas, Dockerfile/Compose, manifiestos de Kubernetes, el flujo de GitHub Actions y toda la documentación de `/docs`.

Esto **no sustituye** la programación en pareja ni convierte a la IA en una tercera integrante: el equipo es responsable de revisar, ejecutar, entender, corregir y poder defender oralmente y en vivo cada parte del sistema, tal como exige el enunciado. "La IA lo generó" no es ni será una explicación aceptable ante una pregunta de la persona docente sobre por qué el código funciona de una manera determinada.

## Ejemplos relevantes de asistencia

- **Diseño del dominio:** modelado de las cinco entidades, la máquina de estados de `Licitacion`, la normalización Unicode de nombres de proveedor y las reglas de mejor oferta/clasificación, siguiendo TDD (prueba en rojo → implementación mínima → refactorización), documentado iteración por iteración en [bitacora-xp.md](bitacora-xp.md).
- **Corrección de errores reales encontrados durante la propia sesión**, no hipotéticos: un conflicto de concurrencia en la activación de tipo de cambio (solo reproducible contra PostgreSQL real), una expresión regular de validación de cliente incompatible con JavaScript, y un uso incorrecto de `SelectList` con una tupla con nombres. Los tres están detallados en [pruebas.md](pruebas.md).
- **Infraestructura:** Dockerfile multi-stage, `docker-compose.yml`, los diez manifiestos de Kubernetes y el flujo de GitHub Actions.

## Validaciones realizadas durante la sesión asistida

Antes de darlas por terminadas, todas las iteraciones se verificaron con ejecuciones reales:

- `dotnet build`/`dotnet test` ejecutados tras cada cambio significativo (0 advertencias al cierre).
- Migraciones aplicadas contra PostgreSQL 16 real (Docker), con inspección directa del esquema resultante por `psql`.
- Flujos de negocio probados manualmente contra la aplicación real corriendo (creación, publicación, rechazo de ofertas, conversión CRC/USD) antes de escribir las pruebas automatizadas equivalentes.
- `docker compose up --build` real, incluida la verificación de persistencia de datos tras `down`/`up`.
- Los diez manifiestos de Kubernetes validados contra el esquema oficial de la versión 1.29 con `kubeconform`.

## Responsabilidad pendiente del equipo (antes de la entrega y la defensa)

La asistencia de IA acelera la primera versión, pero **no reemplaza** la comprensión del equipo. Antes de la entrega final, Kristel Gamboa y Geiner Alfaro deben, en pareja y alternando roles:

1. Leer y ejecutar cada módulo de principio a fin, confirmando que pueden explicar cualquier decisión de diseño sin depender de este documento.
2. Ejercitar en vivo al menos un cambio en cada capa (dominio, aplicación, infraestructura, Web, API) para confirmar dominio práctico del código, no solo lectura pasiva.
3. Revisar que ningún comentario, mensaje o dato de prueba quede como artefacto que insinúe autoría de una herramienta de IA dentro del código entregado (spec §16), más allá de esta declaración explícita.
4. Estar preparados para modificar en vivo cualquier parte del sistema durante la defensa oral, según lo exige el enunciado (§3, "Evaluación individual").

Este documento se debe actualizar si se usa IA en trabajo posterior a esta sesión (por ejemplo, para depurar un hallazgo de la defensa o extender una funcionalidad), indicando la misma información: herramienta, finalidad, módulos, ejemplos y validación.
