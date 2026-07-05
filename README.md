# LOR tryDicing

`tryDicing` is a Library of Ruina mod workspace for experimenting with
automated combat control.

The project currently focuses on:

```text
Reading current battle state.
Enumerating legal player actions.
Evaluating card, target, and speed-dice choices.
Executing validated actions in-game.
Advancing battle flow automatically.
```

## Current Shape

```text
src/tryDicing/
  C# mod source.

src/tryDicing/AutoPlay/
  Card and target selection pipeline.

src/tryDicing/AutoBattle/
  Battle phase automation.

mod/tryDicing/
  Packaged mod layout. Runtime DLLs are local build output and are not tracked.

docs/
  Project status, architecture notes, strategy design, and workflow guidance.

tools/
  Static knowledge exporters and project helper scripts.

generated/static_knowledge/
  Generated summaries may be tracked. Large JSON exports are generated locally
  and are not tracked.
```

## Documentation Entry

Start here:

```text
docs/README.md
docs/codex_entry.md
docs/project_status.md
```

For Codex-specific project rules, see:

```text
AGENTS.md
```

## Generated Data

Large generated JSON files under `generated/static_knowledge/` and decompiled
game source under `generated/decompiled/` are intentionally excluded from Git.

Regenerate static knowledge locally with the tools under:

```text
tools/static_info_exporter/
```

