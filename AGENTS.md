# Main Agent Instructions

## Response Style

- Be concise by default.
- Do not over-explain.
- If the user asks a yes/no question, answer with "Yes" or "No" first, then add only minimal context if needed.
- Do not provide long explanations unless explicitly requested.
- When planing explain more of the patterns tech and code you will use rather then just long text explaining the details. Keep the Details shorter and on point
- When explaining completed work, summarize only key changes.
- Avoid repetition and unnecessary background context.
- Do not include tutorials unless requested.
- If the answer is obvious from the code, keep responses short.
- While planning, if anything is not 100% certain, ask the user before proceeding.
- If a request is ambiguous or could be misunderstood, stop and ask for clarification before acting.
- Do not assume user intent when details are unclear; double-check user-provided facts and call out likely mistakes or better options.

## Repository Guidelines

### Context Usage

- When the user explicitly provides files as context, prefer using only those files.
- Do not scan the repository unnecessarily.
- Do not search unrelated folders unless required for correctness.
- If additional files are needed, explain briefly why before loading them.

### Information Sources

- Prefer repository context first.
- Use relevant MCP servers next when repo context is insufficient.
- Do not ask before using MCP when limited official documentation lookup is clearly relevant.
- Use web last, mainly for current external facts or when MCP does not cover the topic.
- Ask before using web search unless the user explicitly requests external/current information.
- Prefer official or primary sources for framework and platform questions.
- Prefer existing repository patterns unless they clearly conflict with framework best practices, correctness, maintainability, security, or performance.
- If an existing pattern appears problematic or outdated, briefly explain the concern before following or changing it.
- Use MCP documentation to validate uncertain framework/library usage when repository patterns may be incorrect.
- Do not blindly replicate repository patterns without considering correctness and maintainability.

### File-Scoped Tasks

If the user provides an explicit set of files and says the context is complete, treat those files as sufficient context.
Do not search the repository or load unrelated files unless a blocker is discovered in the provided files.
Limit work to the specified files unless the user asks for broader changes.

### Project Overview

SiteWatch is split into:

- .NET Clean Architecture backend under `src/`
- Expo React Native client under `src/ClientApp/`

### Global Rules

- Do not build, run, or launch projects unless explicitly asked.
- Anything that needs testing should be tested.
- Do not create, generate, or modify EF migrations or snapshot files unless the user explicitly asks for that work.
- Do not commit secrets or environment-specific values.
- Keep changes scoped and avoid unrelated formatting or churn.
- Follow existing repository naming and formatting conventions.
- Prefer existing patterns over introducing new abstractions.
- Read only the minimum relevant files required for the task.

### Specialized Guidance

Load specialized instructions only when relevant to the current task.

- `docs/agents/core.md` → shared repository conventions and layout
- `docs/agents/backend.md` → `Api` / `Application` / `Domain` / `Infrastructure`
- `docs/agents/frontend.md` → Expo Router and React Native
- `docs/agents/testing.md` → tests and QA
- `docs/agents/deployment.md` → environment and release concerns

### Instruction Loading Rules

- Do not load all instruction files automatically.
- Load only instructions relevant to the current task/module.
- Prefer folder-local instructions when available.
- Avoid unnecessary repository-wide scans.
