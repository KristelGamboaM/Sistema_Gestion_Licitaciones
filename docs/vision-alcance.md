# Visión y Alcance

## Propósito

El Sistema de Gestión de Licitaciones administra el ciclo de vida de licitaciones públicas: publicación, recepción de ofertas de proveedores, determinación de la mejor oferta y del nivel de aprobación correspondiente, con el colón costarricense (CRC) como moneda oficial y una conversión referencial a dólares (USD) mediante un tipo de cambio administrable localmente.

## Problema que resuelve

Sin un sistema centralizado, el seguimiento manual de licitaciones y ofertas es propenso a: aceptar ofertas fuera de plazo, exceder el presupuesto aprobado, duplicar ofertas de un mismo proveedor, y perder trazabilidad de quién debe aprobar cada monto. El sistema aplica estas reglas de forma consistente en la interfaz, el servidor y la base de datos.

## Alcance funcional (incluido)

- CRUD completo de Licitaciones, Proveedores, Ofertas, Niveles de Aprobación y Tipos de Cambio.
- Máquina de estados de licitación (`Borrador → Publicada → Cerrada`) con cierre funcional automático al vencer la fecha de cierre.
- Validación de unicidad y normalización (código de licitación, nombre de proveedor).
- Cálculo de mejor oferta, porcentaje de ahorro y clasificación.
- Resolución del nivel de aprobación mediante tabla parametrizable de rangos.
- Conversión referencial CRC/USD sin alterar los montos almacenados.
- Interfaz web (MVC, landing page, modo claro/oscuro) y API REST versionada con documentación interactiva.
- Persistencia en PostgreSQL con migraciones, auditoría (`CreatedAt`/`UpdatedAt`/`DeletedAt`) y concurrencia optimista.
- Contenerización con Docker Compose y manifiestos de despliegue en Kubernetes.
- Integración continua con GitHub Actions.

## Fuera de alcance

- Autenticación/autorización de usuarios (no exigida por el enunciado; el nivel de aprobación es informativo, no un flujo de autorización con login).
- Integración con una API externa de tipo de cambio (la tasa se administra localmente, el sistema debe funcionar sin Internet).
- Reapertura de licitaciones cerradas.
- Notificaciones (correo, SMS, etc.).
- Despliegue en un proveedor cloud gestionado; los manifiestos de Kubernetes se documentan y validan localmente (ver [kubernetes.md](kubernetes.md)).

## Moneda y presentación

El CRC es la fuente de verdad para todos los montos almacenados (`numeric(18,2)`, nunca `float`/`double`). El USD es siempre una representación calculada en la capa de presentación (`MontoCRC / CRCporUSD` del tipo de cambio activo), mostrando la fecha de vigencia de la tasa usada. El formato de colones usa la cultura `es-CR`.

## Interesados (stakeholders)

- **Persona docente / Cliente XP:** define y valida los criterios de aceptación; realiza la defensa oral y modificación en vivo.
- **Equipo de desarrollo:** Kristel Gamboa y Geiner Alfaro, responsables conjuntos de todo el sistema.
- **Personas usuarias finales simuladas:** encargadas de compras (gestionan licitaciones y ofertas) y personas administradoras (gestionan niveles de aprobación y tipo de cambio).

## Documentos relacionados

- [historias-usuario.md](historias-usuario.md): detalle funcional verificable.
- [plan-xp.md](plan-xp.md), sobre cómo se organiza el trabajo bajo XP.
- [arquitectura-general.md](arquitectura-general.md): cómo se estructura la solución técnica.
