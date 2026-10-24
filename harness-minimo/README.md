# Minimal harness

A ~200-line Python script that shows what a **harness** is by building one: the same job Claude Code does for the orchestrator in this repo, stripped down to the essentials.

It runs a single agent — the functional analyst — that reads a Jira issue and returns D365 requirements as JSON.

## What a harness is

A harness is **a call to the model inside a loop, plus the code that executes the tools the model asks for.** Everything else is built on top of that.

```
        ┌──────────────────────────────────────────────┐
        │                                              │
task ──►│  call model (system prompt + tools + history)│
        │                    │                         │
        │        stop_reason == "tool_use"?            │
        │           │                    │             │
        │          no                   yes            │
        │           │                    │             │
        │     final answer      approve? ─► run tool   │
        │                                  │           │
        │              tool_result appended to history │
        └──────────────────────────────────────────────┘
```

The model never executes anything. It only replies "I want to call `get_jira_issue` with `{"key": "PROJ-123"}`". The harness runs that function and sends the result back.

## How it maps to the code

| Section in `harness.py` | Responsibility | Claude Code equivalent |
|---|---|---|
| 1. Tools | Declare each tool to the model and implement it | Built-in tools + MCP servers in `.mcp.json` |
| 2. Permissions | Read tools run freely; write tools ask a human | Permission prompts and allow lists |
| 3. Agent definition | System prompt + list of tools | `.claude/agents/*.md` (body + `tools:`) |
| 4. The loop | Call, execute tools, append results, repeat until done or until the turn limit | The agent loop itself |
| `log()` | Trace every turn and tool call | Session transcript |

What this script deliberately leaves out, and Claude Code adds: sub-agents (a tool that starts another loop with a different agent file), context compaction when the history grows too large, skills, hooks, streaming and a user interface.

## Run it

Requires Python 3.10+, an Anthropic API key and a Jira Cloud API token.

```bash
cd harness-minimo
pip install -r requirements.txt

export ANTHROPIC_API_KEY=...
export JIRA_BASE_URL=https://your-site.atlassian.net
export JIRA_EMAIL=you@example.com
export JIRA_API_TOKEN=...          # https://id.atlassian.com/manage-profile/security/api-tokens

python harness.py PROJ-123
```

The trace goes to stderr and the requirements JSON to stdout and to `requisitos-PROJ-123.json`. If the agent wants to post questions on the issue, you will be asked to approve the comment first.

Optional: set `HARNESS_MODEL` to use a different model.

## Exercises

1. Add a tool `write_file(path, content)` and let the agent save its own output. Decide whether it belongs in `WRITE_TOOLS`.
2. Add a second agent (the architect) with its own system prompt and tools, and a tool `delegate_to_architect(requirements)` that runs `run_agent` for it. You have just built sub-agents and an orchestrator.
3. Print the total token usage at the end and stop the loop if it exceeds a budget.
4. Trigger the script from a Jira webhook (for example in an Azure Function) so the analysis runs when an issue moves to a given status, with no one at a terminal.
