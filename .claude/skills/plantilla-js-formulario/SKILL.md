---
name: plantilla-js-formulario
description: Convenciones y ejemplo para escribir web resources JavaScript de formularios y BPF de Dynamics 365 en este proyecto. Úsala siempre antes de crear o modificar JS de formulario.
---

# Plantilla de JavaScript de formularios

Fichero de referencia: `ejemplo.form.js` (en esta carpeta). Léelo antes de escribir.

## Estructura

- Un web resource por formulario de entidad, con un namespace `Proyecto.<Entidad>` (sustituye `Proyecto` por el namespace de `CLAUDE.md`).
- Patrón módulo: funciones privadas y un objeto público solo con los manejadores que se registran en el formulario.
- Manejadores públicos con nombre de evento: `onLoad`, `onChange<Campo>`, `onSave`, `onStageChange`.
- `onLoad` registra los `addOnChange` y llama a las funciones de estado inicial. Así el formulario solo necesita registrar `onLoad`.

## Hacer

- `const formContext = executionContext.getFormContext();` en cada manejador.
- Comprobar `null` en `getAttribute`, `getControl` y en los valores.
- `formContext.ui.getFormType()` para distinguir creación (1), actualización (2), solo lectura (3), etc.
- `Xrm.WebApi.retrieveRecord` / `retrieveMultipleRecords` con `$select` explícito, `async/await` y `try/catch`.
- Errores visibles con `formContext.ui.setFormNotification(mensaje, "ERROR", id)` y limpieza con `clearFormNotification(id)`.
- Validación de guardado en `onSave` con `executionContext.getEventArgs().preventDefault()` y un mensaje claro.
- Constantes con los nombres lógicos de campos y valores de opciones al principio del fichero.

## No hacer

- `Xrm.Page` (obsoleto).
- Manipular el DOM o usar jQuery en formularios de modelo.
- Llamadas síncronas (`XMLHttpRequest` síncrono).
- Lógica que deba cumplirse siempre: eso va en un plugin. El JS solo mejora la experiencia.
- `alert()`. Usa `Xrm.Navigation.openAlertDialog` o notificaciones.
