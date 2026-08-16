#!/usr/bin/env python3
"""Falla si la cobertura de linea no alcanza los umbrales del enunciado (spec 12.4):
Domain+Application >= 80%, proyecto completo >= 70%. Lee el Cobertura.xml combinado
generado por reportgenerator (-reporttypes:Cobertura)."""

import sys
import xml.etree.ElementTree as ET

UMBRAL_DOMAIN_APPLICATION = 80.0
UMBRAL_GLOBAL = 70.0
PAQUETES_DOMAIN_APPLICATION = ("Licitaciones.Domain", "Licitaciones.Application")


def porcentaje_lineas(clases):
    total = cubiertas = 0
    for clase in clases:
        for linea in clase.findall("./lines/line"):
            total += 1
            if int(linea.get("hits", "0")) > 0:
                cubiertas += 1
    return (cubiertas / total * 100) if total else 0.0, total, cubiertas


def main(ruta_xml: str) -> int:
    raiz = ET.parse(ruta_xml).getroot()
    todas_las_clases = raiz.findall(".//packages/package/classes/class")
    global_pct, global_total, global_cub = porcentaje_lineas(todas_las_clases)

    clases_dominio = [
        c
        for c in todas_las_clases
        if any((c.get("filename") or "").replace("\\", "/").find(f"/{p}/") != -1 for p in PAQUETES_DOMAIN_APPLICATION)
    ]
    dominio_pct, dominio_total, dominio_cub = porcentaje_lineas(clases_dominio)

    print(f"Cobertura global: {global_pct:.2f}% ({global_cub}/{global_total} lineas) — umbral {UMBRAL_GLOBAL}%")
    print(
        f"Cobertura Domain+Application: {dominio_pct:.2f}% ({dominio_cub}/{dominio_total} lineas) "
        f"— umbral {UMBRAL_DOMAIN_APPLICATION}%"
    )

    fallo = False
    if global_pct < UMBRAL_GLOBAL:
        print(f"::error::Cobertura global {global_pct:.2f}% por debajo del umbral {UMBRAL_GLOBAL}%")
        fallo = True
    if dominio_total > 0 and dominio_pct < UMBRAL_DOMAIN_APPLICATION:
        print(f"::error::Cobertura Domain+Application {dominio_pct:.2f}% por debajo del umbral {UMBRAL_DOMAIN_APPLICATION}%")
        fallo = True

    return 1 if fallo else 0


if __name__ == "__main__":
    ruta = sys.argv[1] if len(sys.argv) > 1 else "TestResults/Report/Cobertura.xml"
    sys.exit(main(ruta))
