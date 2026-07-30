# SiteWatch agent instructions

## Instruction routing

- Read this root file for every task.
- Identify the paths and architectural areas affected by the request before reading implementation files.
- Read only the nested `AGENTS.md` files whose scopes apply to those paths. Do not enumerate or read unrelated agent files.
- For a task spanning multiple areas, read the nested file for each affected area. Nested instructions add local rules and take precedence within their scope.
- For an explicitly requested test task, also read the instructions for the production area whose behavior is under test.

## Repository map

- `src/Domain`: domain model and innermost backend layer.
- `src/Application`: application use cases and abstractions.
- `src/Infrastructure`: persistence and external-system implementations.
- `src/Api`: ASP.NET Core host, endpoints, middleware, and composition root.
- `src/Dashboard`: Angular administrative application.
- `src/ClientApp`: Expo/React Native client using a development build.
- `src/Api.Tests` and `src/Application.Tests`: test projects; test-source changes require explicit user authorization.
- `docker-compose.yaml`, `sqlserver`, and `azurelite-data`: local infrastructure and persisted service data.

Preserve the proven backend dependency direction: `Domain <- Application <- Infrastructure <- Api`. Do not introduce an outward reference from an inner layer.

## Authorization boundaries

- Default to static inspection and reasoning only.
- Do not build, run, start, test, lint, type-check, format, benchmark, or otherwise execute project code or project tooling unless the user explicitly requests that action.
- Permission to change code does not imply permission to execute it. Permission to execute one command or one project does not extend to other commands or projects.
- Do not create, modify, regenerate, or delete tests unless the user explicitly requests test-source work.
- Do not launch the API, frontend development servers, Expo, emulators, development clients, Docker services, or external infrastructure unless explicitly requested.
- Do not generate, edit, remove, or apply database migrations unless explicitly requested. Any command that mutates a database requires explicit approval for the named environment.
- Reuse existing dependencies when practical. Ask before adding, replacing, or upgrading a production or development dependency unless the request explicitly requires it.
- Supporting edits needed for the requested behavior are allowed within the affected scopes. Ask before broad refactors, unrelated cleanup, or new functionality.

## Knowledge and implementation guidance

- Keep AGENTS files limited to stable architecture, ownership, permissions, and routing. Concrete syntax and implementation procedures belong in applicable skills or focused project documentation.
- Start with repository context and established local patterns. Use the given skills in the promt and only if needed another needed include them to the list. Use the skills applicable to the task; do not load unrelated to the task skills.
- Use the runtime skill catalog and the repository skill sources under `.agents/skills` and `.codex/skills`. Follow a selected skill's instructions completely.
- If repository context and applicable skills leave an unresolved or version-sensitive technical gap or simply have questionable techniques or patterns, consult Context7 when it is available and retrieve only the missing information. Use web search only as the final permitted fallback when Context7 is unavailable or inadequate and current external information is still necessary.
- Do not invent commands. If execution is explicitly requested, derive the exact command from the current solution, project, package scripts, configuration, or documentation and keep it limited to the requested scope.

## Safety and repository hygiene

- Preserve unrelated user changes. Do not create commits or branches, amend, reset, stash, force-checkout, discard changes, or perform destructive Git operations unless explicitly requested.
- Never expose, echo, copy, or commit `.env` contents, tokens, passwords, certificates, private keys, connection strings, or persisted database/storage data.
- Treat `bin`, `obj`, `dist`, `node_modules`, `.angular`, `.expo`, coverage output, generated clients, and similar artifacts as generated. Change their source or generator rather than editing output directly, and regenerate only when authorized.
- Do not modify data under `sqlserver` or `azurelite-data` unless the request explicitly targets that data and the destructive or mutation boundary is clear.
- Update documentation only when the requested change makes existing documented behavior, commands, or architecture inaccurate.

