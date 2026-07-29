---
name: prompt-generator
description: Generate execution-ready prompts from user instructions and identify the repository skills required for the task. Use when the user invokes /prompt or asks to turn an instruction into a prompt for another agent to execute in this repository.
---

# Repository Prompt Generator

When the user invokes `/prompt`, convert the supplied instruction into a precise prompt for an implementation agent. Do not perform the requested task.

## Workflow

1. Extract the requested outcome, affected area, constraints, and expected deliverable.
2. Identify only the repository skills relevant to the task. Use their exact names when known.
3. Read each selected skill's instructions far enough to identify its required inputs, expected decisions, prerequisites, and deliverables. Align the generated prompt with those inputs explicitly.
4. Preserve explicit user requirements and do not invent scope, files, technologies, or acceptance criteria.
5. If any part of the request or a selected skill's required inputs is unclear, ambiguous, conflicting, or not understood, ask concise clarifying questions and wait for the user's answers. Do not generate the prompt or substitute assumptions until the uncertainty is resolved.
6. Read [references/models.md](references/models.md) completely. Recommend one model from that catalog based on the task's complexity, risk, latency, and cost. Do not inspect or depend on a runtime model catalog.
7. Classify task difficulty as low, medium, or high using scope, ambiguity, risk, dependencies, and required depth. Recommend a reasoning level supported by the selected model and execution surface.
8. Recommend whether Plan mode should be used.
9. Choose whether Plan mode or Goal Mode should be used. These are mutually exclusive; recommend at most one, or neither for a small self-contained task.
10. Produce the prompt using the format below.

## Output format

```text
Task
[A concise statement of the requested work]

Required skills
- [Exact relevant skill name, or "None"]

Skill-aligned inputs
- [Input required by a selected skill, its value from the request or repository context, and how the implementing agent should use it]

Execution guidance
- Model: [Exact model ID from the checked-in catalog, with a short reason]
- Difficulty: [low, medium, or high]
- Reasoning: [Supported reasoning level, with a short reason]
- Plan mode: [Yes or no, with a short reason]
- Goal Mode: [Yes or no, with a short reason]

Context
- [Known repository, product, or technical context]

Requirements
- [Concrete implementation requirements]

Constraints
- [User-specified or repository constraints; omit this section when empty]

Expected result
- [Known files, behavior, or response the implementing agent must produce]
```

## Skill selection

- Select skills based on the task domain, tools, files, and requested output.
- Include a skill only when it provides necessary instructions or capabilities.
- Prefer specialized skills over generic ones when both apply.
- Inspect the runtime skill catalog first and use exact names from it. Never invent a skill name or rely on memory when the catalog is available.
- If the task spans multiple domains, list each required skill and explain its role briefly.
- Do not include `prompt-generator` in the generated prompt's Required skills.

## Execution guidance

Use [references/models.md](references/models.md) as the sole model-selection source. Do not query runtime model metadata. Prefer Terra for normal repository work, Luna for simple high-volume or strongly cost-sensitive work, and Sol when complexity, uncertainty, security, architecture, or debugging risk justifies flagship capability.

Classify difficulty consistently:

- Use `low` for simple, localized, well-defined, low-risk work.
- Use `medium` for multi-file or behavioral changes that follow established repository patterns.
- Use `high` for architectural changes, ambiguous requirements, security-sensitive work, difficult debugging, or changes spanning multiple subsystems.

Choose reasoning proportionally:

- Use `none` or `low` for mechanical, localized, low-risk work.
- Use `medium` for normal repository implementation and review work.
- Use `high` for complex debugging, architecture, security-sensitive work, or substantial ambiguity.
- Use `xhigh` or `max` only for exceptional, quality-first work where the added latency and cost are justified.
- Recommend `ultra` only for Codex execution when parallel agent work materially benefits a complex task; never present it as an API reasoning-effort value.

Plan mode and Goal Mode are mutually exclusive. Recommend Plan mode for tasks with multiple dependent steps, several affected layers, repository exploration, or meaningful architectural tradeoffs that can be completed in a bounded execution. Recommend Goal Mode for tasks likely to require sustained work across multiple turns, iterative implementation and verification, broad repository changes, monitoring, or an explicit outcome-oriented request to keep working until completion. Recommend neither for small, localized, well-defined edits, and never recommend both.

## Quality rules

- Generate the prompt only after the request is understood well enough to avoid unresolved assumptions.
- Make the generated prompt self-contained enough for another agent to execute.
- Keep it concise and action-oriented.
- Include execution guidance so the user can choose the appropriate reasoning level, Plan mode, and Goal Mode.
- When skills are selected, include every required skill input in the prompt, mapped to the user's request or discovered repository context. Do not omit required inputs or invent values; ask the user when a required input cannot be safely discovered.
- Do not add unrelated cleanup, testing, deployment, or documentation work unless requested.
- Preserve the repository's `AGENTS.md` and relevant `docs/agents` instructions as part of the implementation context.
- Mention repository paths only when supplied by the user, present in provided context, or discovered from relevant repository context. Do not guess filenames or paths.
