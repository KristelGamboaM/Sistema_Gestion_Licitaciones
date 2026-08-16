# Módulo: Interfaz Web (`Licitaciones.Web`)

## Propósito

Ofrecer una landing page explicativa, navegación a los cinco módulos funcionales, modo claro/oscuro, y un alternador global de moneda (CRC/USD), sobre ASP.NET Core MVC con Bootstrap 5.3 (vendorizado localmente en `wwwroot/lib`, sin depender de una CDN).

## Landing page (§5.1)

`Views/Home/Index.cshtml` explica el flujo de una licitación (publicar → ofertar → aprobar) y enlaza los cinco módulos y la documentación interactiva de la API. Usa las clases responsive de Bootstrap (`row-cols-1 row-cols-md-2 row-cols-lg-3`) para adaptarse a computadora y móvil.

## Modo claro/oscuro

Bootstrap 5.3 soporta modo oscuro nativamente vía el atributo `data-bs-theme` en `<html>`. `wwwroot/js/site.js`:

- Determina el tema inicial: preferencia guardada en `localStorage` o, si no existe, `prefers-color-scheme` del sistema operativo.
- El botón "🌓 Tema" en la barra de navegación alterna y persiste la preferencia.

## Alternador global CRC/USD

Cada monto en colones se marca en las vistas con un atributo `data-monto-crc="<valor>"` (por ejemplo, en `Licitaciones/Index.cshtml`, `Licitaciones/Details.cshtml`, `Ofertas/Index.cshtml`, `NivelesAprobacion/Index.cshtml`). `_Layout.cshtml` inyecta `ITipoCambioAppService` para leer el tipo de cambio **activo** una sola vez por solicitud y lo expone en `data-crc-por-usd`/`data-fecha-vigencia-tipo-cambio` en `<body>`, de modo que el toggle no necesita otra llamada de red.

El botón "Ver en $ USD" en `site.js`:

1. Lee la tasa activa desde los atributos de `<body>`.
2. Reformatea todos los elementos `[data-monto-crc]` con `Intl.NumberFormat` (`es-CR`/`CRC` o `en-US`/`USD`), sin recargar la página.
3. Persiste la preferencia en `localStorage` y se deshabilita automáticamente si no hay tipo de cambio activo configurado.

Los valores originales en CRC nunca se modifican: la conversión es puramente de presentación (spec §8.8), igual que en la capa de aplicación (`TipoCambioAppService.ConvertirCrcAUsdAsync`).

## Validación de formularios

DataAnnotations en los ViewModels (`Models/*/‌*FormViewModel.cs`) + `_ValidationScriptsPartial` (jQuery Validation Unobtrusive, vendorizado) dan validación en cliente; el servidor revalida con `ModelState.IsValid` y además captura `ReglaNegocioException` para mostrar mensajes de negocio junto al formulario (`asp-validation-summary="ModelOnly"`).

## Manejo de errores y mensajes

- Éxito (`TempData["Mensaje"]`) y error (`TempData["Error"]`), renderizados como alertas Bootstrap dismissible en `_Layout.cshtml`, visibles en cualquier página tras una redirección.
- `EntidadNoEncontradaException` se traduce a `404` (`NotFound()`) en los controladores.

## Documentos relacionados

- [tipo-cambio.md](tipo-cambio.md): servicio de conversión que respalda el toggle.
- [../arquitectura-general.md](../arquitectura-general.md), sobre cómo Web depende de Infrastructure/Application.
