---
name: prompt-generator
description: Generate execution-ready prompts from user instructions and identify the repository skills required for the task. Use when the user invokes /prompt or asks to turn an instruction into a prompt for another agent to execute in this repository.
---

# Repository Prompt Generator

When the user invokes `/prompt`, convert the supplied instruction into a precise prompt for an implementation agent. Do not perform the requested task.

## Workflow

1. Extract the requested outcome, affected area, constraints, and expected deliverable.
2. Identify only the repository skills relevant to the task from the active skill catalog's names and descriptions. Do not read candidate `SKILL.md` files during prompt generation.
3. Preserve explicit user requirements and do not invent scope, files, technologies, or acceptance criteria.
4. If any important requirement is unclear, ambiguous, conflicting, or not understood, ask concise clarifying questions and wait for the user's answers. Do not generate the prompt or substitute assumptions until the uncertainty is resolved.
5. Read only the relevant section of [references/models.md](references/models.md) when a model recommendation is requested or an exact model ID is needed. Do not inspect or depend on a runtime model catalog.
6. Recommend task difficulty and reasoning effort using only `low`, `medium`, or `high`; default to `low` for a small, localized task.
7. Recommend Plan mode or Goal Mode only when the task needs it. They are mutually exclusive; recommend at most one, or neither for a small self-contained task.
8. Produce the prompt using the format below.

## Output format

```text
Task
[A concise statement of the requested work]

Required skills
- [Exact relevant skill name, or "None"]

Execution guidance
- Model: [Exact model ID from the checked-in catalog]
- Difficulty: [low, medium, or high]
- Reasoning: [low, medium, or high, when useful]
- Plan mode: [Yes or no, when the task is more than a small localized edit]
- Goal Mode: [Yes or no, when sustained or outcome-oriented work is required]

Context
- [Known repository, product, or technical context]

Requirements
- Before acting, discover and obey the repository-root and every applicable scoped `AGENTS.md` instruction file.
- [Concrete implementation requirements]

Constraints
- [User-specified or repository constraints; omit this section when empty]

Expected result
- [Files, behavior, or response the implementing agent must produce]
```

## Skill selection

- Select skills based on the task domain, tools, files, and requested output.
- Select them from their catalog names and descriptions only; do not load their instruction files during prompt generation.
- Include a skill only when it provides necessary instructions or capabilities.
- Prefer specialized skills over generic ones when both apply.
- Use exact skill names from the repository's available skill catalog. Never invent a skill name.
- If the task spans multiple domains, list each required skill by its exact catalog name.
- Do not include `prompt-generator` in the generated prompt's Required skills.

## Execution guidance

Use [references/models.md](references/models.md) as the sole model-selection source. Do not query runtime model metadata. Prefer Terra for normal repository work, Luna for simple high-volume or strongly cost-sensitive work, and Sol when complexity, uncertainty, security, architecture, or debugging risk justifies flagship capability.

Classify difficulty and reasoning as follows:

- **Low**: simple, localized, well-defined edits.
- **Medium**: multi-file changes or behavior changes using established repository patterns.
- **High**: architectural changes, ambiguous requirements, security-sensitive work, difficult debugging, or changes spanning multiple subsystems.

Recommend Plan mode for tasks with multiple dependent steps, several affected layers, repository exploration, or meaningful architectural tradeoffs that can be completed in a bounded execution. Recommend Goal Mode for tasks likely to require sustained work across multiple turns, iterative implementation and verification, broad repository changes, monitoring, or an explicit outcome-oriented request to keep working until completion. Recommend neither for small, localized, well-defined edits, and never recommend both.

## Token-efficiency rules

- Do not read the whole repository, the full skill catalog, unrelated `AGENTS.md` files, lockfiles, generated output, or every candidate skill merely to generate a prompt.
- Use the user's wording and supplied repository context first. Identify skills by catalog names and descriptions; do not read candidate skill instructions during prompt generation.
- Do not restate repository policy that the implementing agent will receive separately; reference the applicable `AGENTS.md` instead.
- Omit empty sections, repeated requirements, generic implementation advice, and execution guidance that does not affect the task.
- Set a stopping point before researching: once the outcome, affected area, required skills, constraints, and expected result are known, draft the prompt.
- Never browse the web or query external documentation for ordinary repository-local prompt generation.
- Keep the generated prompt compact: prefer one concise sentence per requirement and avoid repeating the task in multiple sections.

## Quality rules

- Generate the prompt only after the request is understood well enough to avoid unresolved assumptions.
- Make the generated prompt self-contained enough for another agent to execute.
- Keep it concise and action-oriented.
- Include execution guidance so the user can choose the appropriate model, reasoning level, Plan mode, and Goal Mode.
- Do not add unrelated cleanup, testing, deployment, or documentation work unless requested.
- Require the implementing agent to discover and obey the repository-root and every applicable scoped `AGENTS.md` instruction file before acting.
- Mention relevant repository paths when they are known from the user's request or provided context.
