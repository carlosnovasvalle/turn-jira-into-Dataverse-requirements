# Contexto del proyecto

> Plantilla. Sustituye los valores entre `<>` por los de tu proyecto. Todos los agentes leen este fichero.

## Cliente y entorno

- Cliente: `<nombre del cliente o proyecto>`
- Producto: Dynamics 365 Customer Engagement / Dataverse (online)
- Solución principal: `<NombreSolucion>`
- Prefijo del publisher: `<pfx>_` (todo campo, entidad o web resource nuevo lo lleva)
- Proyecto de Jira: `<PROJ>`
- Espacio de Confluence con el funcional: `<ESPACIO>`

## Entidades principales

| Nombre visible | Nombre lógico | Notas |
|---|---|---|
| Cuenta | `account` | |
| Contacto | `contact` | |
| `<Entidad propia>` | `<pfx>_entidad` | `<qué representa>` |

## Convenciones de código

- Plugins: C#, .NET Framework 4.6.2, un plugin por clase, heredan de `PluginBase` (ver skill `plantilla-plugin`).
- Namespace de plugins: `<Cliente>.Plugins.<Entidad>`
- JavaScript: un web resource por formulario, namespace `<Cliente>.<Entidad>` (ver skill `plantilla-js-formulario`).
- Ruta de web resources: `<pfx>_/js/<entidad>/<entidad>.form.js`
- Idioma de comentarios y mensajes al usuario: español.

## Criterios del equipo

- Lógica que debe cumplirse siempre, entre por formulario, API o importación → plugin (síncrono si debe bloquear o validar).
- Solo experiencia de usuario en el formulario → JavaScript.
- Integraciones asíncronas con sistemas externos → Power Automate o plugin asíncrono, según `<criterio del proyecto>`.
- Antes de crear un campo nuevo, comprobar si ya existe uno equivalente.

## Rutas del repo

- Código de plugins: `<src/Plugins>`
- Web resources: `<src/WebResources>`
- Salida de propuestas: `propuestas/<CLAVE-JIRA>/`
