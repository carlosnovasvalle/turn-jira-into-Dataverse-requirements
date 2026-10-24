---
name: plantilla-plugin
description: Convenciones, clase base y ejemplo para escribir plugins C# de Dataverse en este proyecto. Úsala siempre antes de crear o modificar un plugin.
---

# Plantilla de plugins de Dataverse

Ficheros de referencia en esta carpeta:

- `PluginBase.cs`: clase base que todos los plugins heredan. Resuelve servicios, tracing y manejo de errores.
- `EjemploPlugin.cs`: plugin completo de ejemplo con las convenciones aplicadas.

Léelos antes de escribir. Si el proyecto ya tiene su propia clase base en el repo, **usa la del proyecto** y no esta.

## Estructura de un plugin

1. Hereda de `PluginBase` e implementa `ExecutePlugin(LocalPluginContext ctx)`.
2. **Valida el contexto primero** y sal pronto si no aplica: mensaje, entidad, etapa y que el Target sea una `Entity`.
3. Lee los valores con `GetAttributeValue<T>`, nunca con el indexador `entity["campo"]`.
4. En Update, el Target solo trae los campos modificados: lo que no venga, léelo de la **PreImage**, no con un Retrieve.
5. Aplica las reglas. Cada una con su comentario `// R1: ...`.
6. Traza las decisiones relevantes con `ctx.Trace(...)`.

## Hacer

- PreOperation: modificar el Target directamente (`target["campo"] = valor`). No hagas `service.Update` del mismo registro.
- Consultas con `ColumnSet` explícito y solo las columnas necesarias.
- `QueryExpression` o FetchXML con filtros. Si puedes, `TopCount` o `top`.
- Mensajes de error al usuario en español y comprensibles: `throw new InvalidPluginExecutionException("No se puede ...")`.
- `ctx.UserService` para actuar como el usuario; `ctx.SystemService` solo si el diseño lo justifica, y anótalo.

## No hacer

- `ColumnSet(true)`.
- GUIDs, URLs o credenciales en el código. Usa variables de entorno o configuración segura.
- Variables de instancia con estado: los plugins se reutilizan entre ejecuciones y no son thread-safe.
- Comprobar `context.Depth > 1` como única protección contra bucles. Usa atributos de filtrado y compara valores antes de actualizar.
- Llamadas HTTP síncronas a sistemas externos desde un plugin síncrono.
- Capturar excepciones y silenciarlas.
