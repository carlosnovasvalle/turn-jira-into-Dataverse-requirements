---
name: checklist-revision
description: Criterios de revisión de propuestas D365 (trazabilidad, plugins, JavaScript y registro). Úsala al revisar cualquier propuesta o código de Dynamics 365.
---

# Checklist de revisión D365

Marca cada punto que falle como hallazgo con la severidad indicada (B = bloqueante, M = mayor, m = menor).

## Trazabilidad

- [B] Cada regla `R*` y punto `UX*` de `requisitos.json` está cubierto por al menos un componente.
- [B] Cada componente `plugin` o `javascript` tiene código, registro y pruebas.
- [B] Toda regla con `debe_cumplirse_siempre: true` está cubierta por un plugin (no solo por JS o business rule).
- [M] Las pruebas cubren los criterios de aceptación de Jira e incluyen al menos un caso negativo.

## Plugins

- [B] El registro del `.registro.md` coincide con `arquitectura.json` y con lo que el código espera (mensaje, etapa, imágenes y sus alias).
- [B] En Update hay atributos de filtrado y son los mínimos necesarios.
- [B] No hay riesgo de bucle: no actualiza el mismo registro desde PostOperation sin comparar valores, ni dispara su propio mensaje sin control.
- [B] Se comprueba `null` antes de usar valores del Target, imágenes o consultas.
- [B] No hay GUIDs, URLs ni credenciales hardcodeados.
- [B] No hay estado en variables de instancia.
- [M] En PreOperation se modifica el Target en lugar de hacer Update del mismo registro.
- [M] No hay `ColumnSet(true)`; las consultas tienen filtros.
- [M] Hay tracing en las decisiones relevantes y los errores llegan al usuario con un mensaje comprensible.
- [M] Los valores no presentes en el Target se leen de la PreImage, no con un Retrieve extra.
- [m] Cada regla lleva su comentario `// R*`. Nombres y namespace según `CLAUDE.md`.

## JavaScript

- [B] No se usa `Xrm.Page`.
- [B] Se comprueba `null` en `getAttribute`, `getControl` y valores.
- [B] Las llamadas a `Xrm.WebApi` tienen manejo de error visible.
- [M] `$select` explícito en las consultas.
- [M] Se contempla el formulario de creación (`getFormType() === 1`).
- [M] No hay lógica que deba cumplirse siempre implementada solo en JS.
- [m] Namespace, constantes y comentarios `// UX*` según la plantilla.

## Configuración y diseño

- [M] Los campos nuevos usan el prefijo del publisher y no duplican campos existentes.
- [M] Las estimaciones son coherentes con la complejidad del componente.
- [m] Los riesgos identificados por el arquitecto tienen una mitigación o están anotados como pendientes.
