# SiteWatch Agent Instructions

## Scope

These instructions apply to the whole repository. Nested `AGENTS.md` files add rules for their subtree and do not replace this file.

## Required evidence workflow

This workflow and its ordering are mandatory for every task and repository scope. Move to a later evidence source only when the earlier sources do not adequately answer the question; do not rely on model memory alone.

1. **Skills first**
   - Before inspecting implementation details, check `.codex/skills/INDEX.md` and the active environment's skill catalog.
   - Read every applicable skill completely and use it as the primary implementation guidance. Compose all relevant skills when a task crosses capabilities such as EF Core, mapping, API work, Angular, or Expo; do not load unrelated skills.
   - Keep reusable technology procedures in skills rather than duplicating them in `AGENTS.md` or improvising from memory.
2. **Project context next**
   - Inspect the applicable `AGENTS.md` files, manifests, configuration, documentation, tests, and representative source before deciding how the repository behaves. Repository evidence is authoritative for local architecture, versions, conventions, and commands.
   - First determine whether this evidence is clear, internally consistent, current enough for the task, and supported by existing usage or tests. If it is adequate, use it and do not query Context7 merely to repeat known repository facts.
3. **Context7 when project evidence needs support**
   - Use Context7 MCP when repository guidance is missing, inconsistent, uncertain, unfamiliar, potentially stale, or depends on current library, framework, SDK, API, CLI, configuration, migration, or integration behavior.
   - Give Context7 extra weight for hard, integration-heavy, or version-sensitive work, but query it narrowly for the specific uncertainty after evaluating project context. Do not use it indiscriminately.
   - For security or performance work that depends on external libraries, frameworks, runtimes, databases, SDKs, protocols, or platform behavior, use Context7 to validate current guidance even when the repository contains a similar implementation. Existing code proves local precedent, not that the precedent is currently safe or efficient. Keep the query limited to the relevant security or performance concern.
   - Resolve the exact library and relevant version before querying. Reconcile the result with repository evidence: Context7 is authoritative for current external API behavior, while the repository remains authoritative for local architecture, versions, conventions, and user-approved policy.
   - If current documentation conflicts with repository practice, identify the mismatch and choose or ask based on its architectural and compatibility impact rather than silently modernizing code.
4. **Web only as the final fallback**
   - Use general web search only after applicable skills and project evidence are inadequate and Context7 is unavailable, insufficient, or returns no adequate answer.
   - Do not use web search in parallel with these earlier stages or as a shortcut around them. Prefer official and primary sources, and state when the result depends on external evidence.

## Repository map

- `src/Api`: ASP.NET Core transport, middleware, and endpoint composition.
- `src/Application`: use cases, commands, queries, validation, and interfaces implemented by outer layers.
- `src/Domain`: entities, value objects, and domain rules.
- `src/Infrastructure`: persistence and external integrations. Its local instructions govern migrations, generated clients, and runtime data.
- `src/Application.Tests`: backend tests.
- `src/ClientApp`: Expo/React Native application with local instructions.
- `src/Dashboard`: Angular application with local instructions.
- `docker-compose.yaml`, `sqlserver`, and `azurelite-data`: local SQL Server and Azurite infrastructure and runtime state.
- `doc`: external integration documentation. `scripts`: repository maintenance utilities.

## Architecture

- Preserve the backend project-reference direction: `Api -> Infrastructure -> Application -> Domain`. Do not introduce references from inner projects to outer projects without an explicit architecture change.
- Keep transport concerns in `Api`, use-case orchestration and contracts in `Application`, domain behavior in `Domain`, and persistence or external-service implementations in `Infrastructure`.
- Treat the Expo client and Angular dashboard as separate deployable applications; do not couple their internal modules or package management.
- Use each project file as the target-framework source of truth and avoid incidental framework-version changes. The intended backend baseline is currently .NET 10. If a project differs or a mismatch blocks verification, report it instead of silently changing target frameworks.

## Working policy

- Start with task-relevant files and search the relevant scope for reusable implementations before adding new ones. Supporting edits required for the same requested behavior are allowed; ask before new functionality, broad refactors, or unrelated cleanup.
- Reuse existing packages first. Ask before adding any production or development dependency unless the user explicitly requested that dependency.
- Add or update focused tests for behavioral changes when practical. Running existing tests is separate from permission to change test source.
- Update documentation when a change alters behavior, commands, or architecture that existing documentation describes.
- Preserve unrelated working-tree changes. Do not create commits or branches unless requested, and do not amend, reset, stash, force-checkout, or discard changes without explicit approval.
- Never expose, echo, copy, or commit secrets, credentials, tokens, certificates, private keys, or sensitive environment values.

## Verification and execution boundaries

- Run relevant targeted build, lint, typecheck, and existing-test checks after changes. Prefer the smallest affected scope and avoid expensive repository-wide checks for trivial edits.
- Use only commands evidenced by repository manifests, scripts, or documentation. Report checks that could not be run or that are blocked by project configuration.
- Do not leave watch-mode or other long-running verification processes running.
- Do not start the API, Expo, Angular development servers, emulators, Docker services, or external infrastructure unless explicitly requested.
- API startup is a database-mutating operation in development: it applies EF migrations and seeds data. Never start it merely to verify a build.
- Require explicit approval before connecting to or mutating any database, external infrastructure, staging environment, or production environment.

## Generated and destructive changes

- Do not manually edit generated outputs such as `bin`, `obj`, `dist`, coverage output, framework caches, native build output, or generated service clients. Change the source or generator and regenerate only when authorized.
- Migration files and snapshots follow the stricter policy in `src/Infrastructure/AGENTS.md`.
- Require explicit approval before deleting substantial data or directories, dropping databases, applying destructive migrations, overwriting environment configuration, or performing destructive Git or infrastructure operations.
