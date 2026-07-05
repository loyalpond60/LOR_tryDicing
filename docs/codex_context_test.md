# Codex Context Smoke Test

Purpose: give Codex a repeatable way to verify that the project remains fast to
read after documentation, generated data, or tooling grows.

## When To Run

Run this after changes that affect project structure, generated files, docs, or
tooling:

```powershell
powershell -ExecutionPolicy Bypass -File tools/test_context_budget.ps1
```

## What It Checks

```text
rg is available.
Default rg file count stays under the context budget.
Heavy paths are not visible in default rg output.
Core entry files exist.
tools/context_inventory.ps1 still runs.
```

The default budget is intentionally loose:

```text
120 files
```

Current expected count is much lower. The limit leaves room for normal source and
documentation growth without letting generated/vendor areas slip back into the
default search set.

## How Codex Should Use It

When asked to test context optimization, Codex should run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/test_context_budget.ps1
```

If it fails, Codex should inspect the failed paths and update one of:

```text
.rgignore
.gitignore
docs/codex_entry.md
tools/context_inventory.ps1
```

If new files are intentionally part of the normal working set, Codex should
raise the budget only after explaining why.

