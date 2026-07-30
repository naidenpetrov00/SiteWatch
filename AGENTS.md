# SiteWatch Agent Instructions

## Scope and evidence

These instructions apply repository-wide. Nested `AGENTS.md` files add local rules.

- Start with the current active skill catalog, including repository-local skills when listed, and select only skills that own the requested capability. Read each selected skill completely; related-skill metadata does not activate another skill by itself.
- `.codex/skills` contains the repository-local skills; do not infer or load removed skill folders. For skill maintenance, use the applicable skill from the active catalog.
- Then read the applicable `AGENTS.md` and one to three task-relevant files. Expand to manifests, tests, documentation, or adjacent source only to answer a concrete uncertainty.
- Repository evidence owns local architecture, versions, conventions, and commands. Use Context7 only for unresolved version-specific external behavior, and general web search only when that remains necessary and Context7 is unavailable or insufficient.
- Do not routinely load lockfiles, generated output, binaries, caches, unrelated directories, or whole registries.

## Structure and architecture

- Preserve `Api -> Infrastructure -> Application -> Domain`. Keep transport in `Api`, use-case orchestration and capability interfaces in `Application`, domain behavior in `Domain`, and persistence or external adapters in `Infrastructure`.
- `src/ClientApp` is an Expo/React Native application and `src/Dashboard` is an Angular application. Treat them as separate deployables and follow their local instructions.
- `src/Infrastructure/AGENTS.md` governs migrations, generated ONVIF clients, runtime data, and external operations.
- `docker-compose.yaml`, `sqlserver`, and `azurelite-data` are local infrastructure or runtime state; `doc` contains integration documentation and `scripts` contains maintenance utilities.
- Use project files as framework-version truth. The backend baseline is .NET 10; report mismatches instead of retargeting incidentally.

## Change and approval policy

- Keep changes within the requested behavior and preserve unrelated working-tree changes. Ask before broader functionality, architecture changes, or unrelated cleanup.
- Reuse existing packages. Adding or updating a production or development dependency requires explicit approval unless the user requested it.
- Creating or changing test source requires an explicit request separate from permission to inspect or execute existing tests.
- Update documentation only when the requested change alters behavior, commands, or architecture it describes.
- Do not create commits or branches, or amend, reset, stash, force-checkout, or discard changes, without explicit approval.
- Never expose, echo, copy, or commit secrets, credentials, tokens, certificates, private keys, or sensitive environment values.

## Execution, generated files, and external systems

- Do not build, run, test, lint, typecheck, format, start, or execute repository verification unless the user explicitly requests the exact execution.
- Do not start the API, Expo, Angular, emulators, containers, or external infrastructure without an explicit request. Connecting to or mutating a database, device, staging environment, or production environment requires explicit approval.
- Do not hand-edit generated output, including `bin`, `obj`, `dist`, coverage, framework caches, native build output, or generated service clients. Change its source or generator only when authorized.
- Migration artifacts and snapshots follow `src/Infrastructure/AGENTS.md`.
- Require explicit approval before deleting substantial data or directories, dropping databases, applying destructive migrations, overwriting environment configuration, or performing destructive Git or infrastructure operations.
