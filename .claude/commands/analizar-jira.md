---
description: Analiza una historia de Jira y genera una propuesta técnica D365 (requisitos, arquitectura, código y borrador para Jira)
argument-hint: CLAVE-JIRA (por ejemplo PROJ-123)
allowed-tools: Read, Write, Glob, Grep, Bash(git checkout:*), Bash(git branch:*), Bash(mkdir:*), Task, Agent, mcp__atlassian__getJiraIssue, mcp__atlassian__addOrEditJiraIssueComment, mcp__atlassian__createJiraIssue, mcp__atlassian__transitionJiraIssue
---

Eres el **orquestador**. Tu trabajo es coordinar subagentes y mantener el estado; no analizas ni programas tú mismo.

Clave de Jira: **$ARGUMENTS**
Carpeta de trabajo: `propuestas/$ARGUMENTS/`

Sigue estos pasos en orden. No te saltes ninguno. Después de cada paso, di en una línea qué ha pasado.

## Paso 0 — Preparación

1. Si `$ARGUMENTS` está vacío, pide la clave y para.
2. Crea `propuestas/$ARGUMENTS/` y `propuestas/$ARGUMENTS/codigo/`.
3. Si el repo es git, crea o cambia a la rama `feature/$ARGUMENTS`.

## Paso 1 — Análisis funcional

Delega en el subagente `analista-funcional` con este encargo:
"Analiza la incidencia $ARGUMENTS y escribe propuestas/$ARGUMENTS/requisitos.json".

Cuando termine, lee `requisitos.json`.

## Paso 2 — Ambigüedades

Si `ambiguedades` contiene alguna con `"bloqueante": true`:

1. Redacta en `propuestas/$ARGUMENTS/jira-borrador.md` un comentario para Jira dirigido al funcional, con las preguntas numeradas y, para cada una, qué parte del diseño depende de ella.
2. Muéstramelo y pregunta si lo publico.
3. Solo si respondo que sí, publícalo con `addOrEditJiraIssueComment`.
4. **Para aquí.** No continúes con el diseño sobre suposiciones.

Si solo hay ambigüedades no bloqueantes, continúa y arrástralas al borrador final.

## Paso 3 — Arquitectura

Delega en `arquitecto-d365`:
"Lee propuestas/$ARGUMENTS/requisitos.json y escribe propuestas/$ARGUMENTS/arquitectura.json".

Lee el resultado. Si algún componente tiene `"tipo": "pendiente"`, trátalo como una ambigüedad bloqueante (vuelve al paso 2).

## Paso 4 — Desarrollo

Para cada componente de `arquitectura.json`:

- `plugin` → subagente `dev-plugin`
- `javascript` → subagente `dev-js-formularios`
- `power-automate`, `business-rule`, `configuracion` → no generan código: solo se describen en el borrador final.

Pasa a cada subagente el `id` del componente y la ruta `propuestas/$ARGUMENTS/`. Lanza en paralelo los que sean independientes.

## Paso 5 — Revisión

Delega en `revisor`:
"Revisa propuestas/$ARGUMENTS/ y escribe propuestas/$ARGUMENTS/revision.json".

Si hay hallazgos con `"severidad": "bloqueante"`, devuelve cada uno al subagente que generó ese componente con el hallazgo concreto y vuelve a revisar **una sola vez**. Si siguen fallando, no insistas: anótalos en el borrador como pendientes.

## Paso 6 — Borrador para Jira

Escribe `propuestas/$ARGUMENTS/jira-borrador.md` con:

1. **Comentario para la historia:** resumen funcional en 3–5 líneas, tabla de componentes (tipo, entidad, evento, estimación) y ambigüedades no bloqueantes.
2. **Subtareas a crear:** una por componente, con título `[<tipo>] <descripción corta>`, descripción y estimación en horas.
3. **Transición propuesta:** el estado al que movería la historia.
4. **Pendientes de revisión** si los hay.

## Paso 7 — Confirmación y publicación

Muéstrame el borrador completo y pregunta, por separado:

- ¿Publico el comentario?
- ¿Creo las subtareas?
- ¿Hago la transición?

Ejecuta **solo** lo que confirme explícitamente. Nunca escribas en Jira sin un "sí" para esa acción concreta.

## Paso 8 — Cierre

Resume en pocas líneas: qué se ha generado (ficheros), qué se ha publicado en Jira y qué queda pendiente.
