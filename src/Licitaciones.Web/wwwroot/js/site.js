// Preferencias de tema (claro/oscuro) y moneda (CRC/USD), persistidas en
// localStorage. Los montos en colones se marcan en las vistas con el
// atributo data-monto-crc; este script los reformatea sin recargar la
// página cuando la persona usuaria alterna la moneda.
(function () {
    "use strict";

    var CLAVE_TEMA = "licitaciones.tema";
    var CLAVE_MONEDA = "licitaciones.moneda";

    function temaPreferido() {
        var guardado = localStorage.getItem(CLAVE_TEMA);
        if (guardado === "light" || guardado === "dark") {
            return guardado;
        }
        return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function aplicarTema(tema) {
        document.documentElement.setAttribute("data-bs-theme", tema);
        localStorage.setItem(CLAVE_TEMA, tema);
    }

    function formatearCRC(monto) {
        return new Intl.NumberFormat("es-CR", { style: "currency", currency: "CRC", minimumFractionDigits: 2 }).format(monto);
    }

    function formatearUSD(monto) {
        return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", minimumFractionDigits: 2 }).format(monto);
    }

    function tipoCambioActivo() {
        var tasa = parseFloat(document.body.dataset.crcPorUsd || "");
        if (!tasa || isNaN(tasa)) {
            return null;
        }
        return { crcPorUsd: tasa, fechaVigencia: document.body.dataset.fechaVigenciaTipoCambio || "" };
    }

    function aplicarMoneda(moneda) {
        var activo = tipoCambioActivo();
        var usarUsd = moneda === "USD" && activo !== null;

        document.querySelectorAll("[data-monto-crc]").forEach(function (elemento) {
            var montoCrc = parseFloat(elemento.getAttribute("data-monto-crc"));
            if (isNaN(montoCrc)) {
                return;
            }
            elemento.textContent = usarUsd ? formatearUSD(montoCrc / activo.crcPorUsd) : formatearCRC(montoCrc);
        });

        var boton = document.getElementById("toggle-moneda");
        if (boton) {
            boton.textContent = usarUsd ? "Ver en ₡ CRC" : "Ver en $ USD";
            boton.disabled = activo === null;
            boton.title = activo === null
                ? "No hay un tipo de cambio activo configurado"
                : "Tipo de cambio vigente desde " + activo.fechaVigencia;
        }

        localStorage.setItem(CLAVE_MONEDA, usarUsd ? "USD" : "CRC");
    }

    function monedaPreferida() {
        return localStorage.getItem(CLAVE_MONEDA) === "USD" ? "USD" : "CRC";
    }

    document.addEventListener("DOMContentLoaded", function () {
        aplicarTema(temaPreferido());
        aplicarMoneda(monedaPreferida());

        var botonTema = document.getElementById("toggle-tema");
        if (botonTema) {
            botonTema.addEventListener("click", function () {
                var actual = document.documentElement.getAttribute("data-bs-theme");
                aplicarTema(actual === "dark" ? "light" : "dark");
            });
        }

        var botonMoneda = document.getElementById("toggle-moneda");
        if (botonMoneda) {
            botonMoneda.addEventListener("click", function () {
                aplicarMoneda(monedaPreferida() === "USD" ? "CRC" : "USD");
            });
        }
    });
})();
