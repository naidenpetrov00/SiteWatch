# Main Agent Instructions

## Response Style

- Be concise by default.
- Do not over-explain.
- If the user asks a yes/no question, answer with "Yes" or "No" first, then add only minimal context if needed.
- Do not provide long explanations unless explicitly requested.
- When creatign a feature try to find the things that can be separated from this to a reusable separation. I am talking about backend and frontend both logic and ui 
- If you think additional scope should be included, stop and ask before you draft or expand the plan.
- Do not include tutorials unless requested.
- While planning, if anything is not 100% certain, ask the user before proceeding.
- If a request is ambiguous or could be misunderstood, stop and ask for clarification before acting.
- Do not assume user intent when details are unclear; double-check user-provided facts and call out likely mistakes or better options.

## Repository Guidelines

### Context Usage

- When the user explicitly provides files as context, prefer using only those files.
- Do not scan the repository unnecessarily.
- Do not search unrelated folders unless required for correctness.

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
- Do not generate or plan testing unless the user explicitly asks for it.
- Do not create, generate, or modify EF migrations or snapshot files unless the user explicitly asks for that work.
- Do not commit secrets or environment-specific values.
- Keep changes scoped and avoid unrelated formatting or churn.
- Follow existing repository naming and formatting conventions.
- Prefer reusing existing functions, methods, utilities, components, services, and patterns over creating new implementations or abstractions.
- Before adding a helper or repeating logic, search the relevant backend or frontend scope for an existing implementation that can be reused.
- When new logic is clearly reusable, or when an existing implementation is duplicated or belongs in a broader/correct scope, separate or move it to the appropriate shared scope and update callers to reuse it.
- Keep abstractions focused and avoid extracting one-off logic or creating layers without a concrete reuse case.
- Avoid building layers for one-off validation or transformation logic.
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
