---
name: agents-file-generator
description: Analyze and create, review, restructure, or refresh a repository's AGENTS.md instruction hierarchy. Use when changing repository agent policy, scoped instructions, instruction ownership, or routing between AGENTS files, skills, and documentation.
---

# Agents File Generator

Create the smallest persistent AGENTS hierarchy that communicates repository operation, stable architecture, skill routing, verified commands, and approval boundaries. Keep implementation procedures in skills and explanatory material in documentation.

## Route by phase

1. Read [discovery.md](references/discovery.md) before inspecting repository structure, commands, instructions, or dirty state.
2. For an existing hierarchy, read [classification-and-hierarchy.md](references/classification-and-hierarchy.md) before classifying rules or proposing scopes.
3. Read [policy-interview.md](references/policy-interview.md) only when source evidence cannot determine a material user-policy choice.
4. Read [outputs-and-validation.md](references/outputs-and-validation.md) before presenting a hierarchy, policy summary, validation record, or completion report.

## Workflow

1. Select existing, new, or targeted-refresh mode.
2. Map each instruction file to its effective scope and parent. Record skills explicitly named by the user as required inputs, then independently check whether they cover the task; add only the additional applicable skills needed for uncovered capabilities. Verify commands and architecture from manifests, documentation, configuration, and representative boundaries.
3. Classify rules as keep in AGENTS, move to skill, move to documentation, deduplicate, rewrite, delete as noise, or ask the user.
4. Propose the root and any necessary nested files. Nested files must contain stable local deltas, not parent repetition.
5. Resolve only policy choices that materially affect behavior, safety, scope, cost, or approvals. Present a concise policy summary.
6. Wait for explicit approval of hierarchy and material policy before writing.
7. Update only approved scopes, then validate parent/child behavior and representative repository tasks.

## Boundaries

- Preserve unrelated changes and deleted or renamed instruction files.
- Confirm commands from repository evidence; never invent build, test, lint, migration, or run commands.
- Do not create `AGENTS.override.md` without a demonstrated replacement need.
- Report detailed implementation knowledge as a skill candidate; do not create the skill unless requested.
- In generated root instructions, include the approved research order: project context first; user-named skills as required inputs without assuming they provide complete coverage; only necessary additional applicable skills next; Context7 for unresolved or version-sensitive gaps and questionable, stale, or legacy technical patterns; then web search as the final permitted fallback.

Complete the work only when the hierarchy, policies, command evidence, skill routing, and representative scenarios are consistent and approved.
