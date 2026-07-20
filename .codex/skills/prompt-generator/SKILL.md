---
name: prompt-generator
description: Generate execution-ready prompts from user instructions and identify the repository skills required for the task. Use when the user invokes /prompt or asks to turn an instruction into a prompt for another agent to execute in this repository.
---

# Repository Prompt Generator

When the user invokes `/prompt`, convert the supplied instruction into a precise prompt for an implementation agent. Do not perform the requested task.

## Workflow

1. Extract the requested outcome, affected area, constraints, and expected deliverable.
2. Identify only the repository skills relevant to the task. Use their exact names when known.
3. Preserve explicit user requirements and do not invent scope, files, technologies, or acceptance criteria.
4. If an important requirement is missing, state the assumption in the generated prompt. Ask a question only when proceeding would materially change the task.
5. Recommend task difficulty and reasoning effort using only `low`, `medium`, or `high`.
6. Recommend whether Plan mode should be used.
7. Produce the prompt using the format below.

## Output format

```text
Task
[A concise statement of the requested work]

Required skills
- [Exact relevant skill name, or "None"]

Execution guidance
- Difficulty: [low, medium, or high]
- Reasoning: [low, medium, or high, with a short reason]
- Plan mode: [Yes or no, with a short reason]

Context
- [Known repository, product, or technical context]

Requirements
- [Concrete implementation requirements]

Constraints
- [User-specified or repository constraints; omit this section when empty]

Expected result
- [Files, behavior, or response the implementing agent must produce]
```

## Skill selection

- Select skills based on the task domain, tools, files, and requested output.
- Include a skill only when it provides necessary instructions or capabilities.
- Prefer specialized skills over generic ones when both apply.
- Use exact skill names from the repository's available skill catalog. Never invent a skill name.
- If the task spans multiple domains, list each required skill and explain its role briefly.
- Do not include `prompt-generator` in the generated prompt's Required skills.

## Execution guidance

Classify difficulty and reasoning as follows:

- **Low**: simple, localized, well-defined edits.
- **Medium**: multi-file changes or behavior changes using established repository patterns.
- **High**: architectural changes, ambiguous requirements, security-sensitive work, difficult debugging, or changes spanning multiple subsystems.

Recommend Plan mode for tasks with multiple dependent steps, several affected layers, repository exploration, or meaningful architectural tradeoffs. Recommend no Plan mode for small, localized, well-defined edits.

## Quality rules

- Make the generated prompt self-contained enough for another agent to execute.
- Keep it concise and action-oriented.
- Include execution guidance so the user can choose the appropriate reasoning level and Plan mode.
- Do not add unrelated cleanup, testing, deployment, or documentation work unless requested.
- Preserve the repository's `AGENTS.md` and relevant `docs/agents` instructions as part of the implementation context.
- Mention relevant repository paths when they are known from the user's request or provided context.
