# turn-jira-into-Dataverse-requirements

An agent orchestrator for **Claude Code** that turns a **Jira** story into a technical proposal for **Dynamics 365 CE / Dataverse**: structured requirements, an architecture decision (plugin, form JavaScript, Power Automate, business rule or configuration), reviewed starter code and — after your confirmation — a comment and sub-tasks posted back to Jira.

```
/analizar-jira PROJ-123
   │
   ▼
Orchestrator ──► analista-funcional ──► requisitos.json (+ open questions)
   │                    │
   │      blocking questions? ── yes ──► questions to the business analyst in Jira → STOP
   ▼
arquitecto-d365 ──► arquitectura.json (one entry per component, with rationale and estimate)
   ▼
dev-plugin  ║  dev-js-formularios        (in parallel, each with its template skill)
   ▼
revisor ──► revision.json (D365 checklist)
   ▼
Orchestrator ──► [human confirmation] ──► Jira: comment + sub-tasks + transition
```

> The agent prompts, skills and generated output are written in **Spanish**, because the target teams work in Spanish. The structure is language-agnostic: translate the `.md` files to adapt it.

## Concepts

| Piece | What it is | Where it lives |
|---|---|---|
| **Harness** | The program that runs the model loop: builds the prompt, exposes tools, executes tool calls, enforces permissions and decides when to stop. Here it is **Claude Code**. | Outside the repo (you install it) |
| **Agent / sub-agent** | A system prompt plus a subset of tools. The `.md` file is only its configuration; the harness turns it into an agent when invoked. | `.claude/agents/*.md` |
| **Tools** | Functions the harness actually executes (`Read`, `Write`, `Bash`, or Jira operations via MCP). An agent file does **not define** tools; it **restricts** them with `tools:`. | Built into Claude Code + `.mcp.json` |
| **Skills** | Knowledge and templates an agent loads when needed. They add no capabilities; they teach how to use the existing ones. | `.claude/skills/*/SKILL.md` |
| **Orchestrator** | The flow that splits the work across sub-agents. Here it is a command with fixed steps (a deterministic workflow with an LLM inside each step). | `.claude/commands/analizar-jira.md` |
| **Project context** | What every agent must know about the client and its conventions. | `CLAUDE.md` |

## Layout

```
.
├── CLAUDE.md                              ← fill in with your project data
├── .mcp.json                              ← Atlassian MCP server (Jira + Confluence)
├── .claude/
│   ├── commands/
│   │   └── analizar-jira.md               ← orchestrator: /analizar-jira KEY
│   ├── agents/
│   │   ├── analista-funcional.md          ← read-only access to Jira/Confluence
│   │   ├── arquitecto-d365.md             ← decides the component type
│   │   ├── dev-plugin.md                  ← writes C# plugins
│   │   ├── dev-js-formularios.md          ← writes form / BPF JavaScript
│   │   └── revisor.md                     ← reviews against a checklist
│   └── skills/
│       ├── plantilla-plugin/              ← conventions + PluginBase.cs + example
│       ├── plantilla-js-formulario/       ← conventions + web resource example
│       └── checklist-revision/            ← D365 review criteria
└── propuestas/                            ← output: one folder per Jira key
```

## Contracts between agents

Agents never pass free text to each other: each one writes a JSON file in `propuestas/<KEY>/` and the next one reads it. Every step is traceable and you can rerun only the one that failed.

| File | Written by | Read by |
|---|---|---|
| `requisitos.json` | analista-funcional | architect, developers, reviewer |
| `arquitectura.json` | arquitecto-d365 | developers, reviewer, orchestrator |
| `codigo/` | dev-plugin, dev-js-formularios | reviewer |
| `revision.json` | revisor | orchestrator |
| `jira-borrador.md` | orchestrator | you, before anything is posted to Jira |

Each schema is described inside the agent that writes it.

## Getting started

1. **Install Claude Code** and open this repo (or copy `.claude/`, `.mcp.json` and `CLAUDE.md` into your D365 solution repo).
2. **Fill in `CLAUDE.md`** with the publisher prefix, main tables and client conventions.
3. **Connect Atlassian.** `.mcp.json` registers Atlassian's remote MCP server under the name `atlassian`. The first time, Claude Code opens the OAuth login. Run `/mcp` to check it is connected and that its tools are named `mcp__atlassian__...`. If the server URL has changed, update it from Atlassian's documentation.
4. **Run** `/analizar-jira PROJ-123`.

### Optional: real Dataverse metadata

If you also connect a Dataverse MCP server pointing at a development environment, add its read tools to the `tools:` list of `arquitecto-d365` and `revisor`. They will then check that tables and columns exist instead of guessing logical names. Always use a development environment and a read-only user.

## Safety

- Sub-agents **only read** Jira and Confluence. Jira write tools are allowed only in the orchestrator, and it **always asks for confirmation** before commenting, creating sub-tasks or transitioning issues.
- No agent deploys to Dataverse. Generated code stays in `propuestas/<KEY>/codigo/` for you to review and integrate.
- Do not commit the contents of `propuestas/` from real projects to a public repo: it contains client requirements. It is excluded in `.gitignore`.

## Customising

- **Architecture criteria:** edit the decision table in `.claude/agents/arquitecto-d365.md`.
- **Your own code templates:** replace the files in `.claude/skills/plantilla-plugin/` and `.claude/skills/plantilla-js-formulario/`.
- **A new component type** (for example Custom API or PCF): create a `dev-<type>.md` agent and its template skill, and add the case to step 4 of the orchestrator.

## License

No license has been chosen yet. Until one is added, all rights are reserved by the author.
