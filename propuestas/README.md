# propuestas/

Aquí el orquestador crea una carpeta por clave de Jira:

```
propuestas/PROJ-123/
├── requisitos.json      ← analista-funcional
├── arquitectura.json    ← arquitecto-d365
├── codigo/
│   ├── plugins/         ← dev-plugin (.cs, .registro.md, .pruebas.md)
│   └── webresources/    ← dev-js-formularios (.js, .registro.md, .pruebas.md)
├── revision.json        ← revisor
└── jira-borrador.md     ← orquestador (lo revisas antes de publicar en Jira)
```

El contenido real está excluido en `.gitignore` porque contiene requisitos de clientes.
