# Repository discovery

Use this guide before proposing policies or instruction scopes. Gather enough evidence to understand repository operation and architecture without reading the entire codebase.

## Discovery sequence

1. Identify and read skills applicable to the AGENTS-generation task through the repository's actual skill discovery mechanism; do not load unrelated skills.
2. Resolve the repository root, current working tree, and applicable instruction files.
3. List top-level directories and key manifests.
4. Map logical applications, deployable units, libraries, shared areas, and dependency boundaries.
5. Discover commands from authoritative scripts, manifests, CI, and documentation.
6. Locate architecture records, skills, generated artifacts, migrations, secrets-related configuration, and external infrastructure.
7. Inspect representative source only to resolve boundaries that metadata and documentation do not prove.

If applicable skills and repository context leave a technical gap, use Context7 MCP only when available and only for the missing information. Use general web search only if Context7 is unavailable or inadequate, current external information remains necessary, and search is permitted. This research order guides the generator; it is not generated AGENTS content by default.

Prefer fast filename and text searches such as `rg --files` and focused `rg` patterns. Avoid recursive content dumps.

## Evidence checklist

Check only applicable categories:

- .NET: `*.sln`, `*.slnx`, `*.csproj`, `Directory.Build.*`, `Directory.Packages.props`, `global.json`.
- JavaScript or TypeScript: `package.json`, lock files, workspace files, `angular.json`, lint, format, and typecheck configuration.
- Expo or mobile: `app.json`, `app.config.*`, `eas.json`, native project configuration.
- Containers and orchestration: `Dockerfile*`, `docker-compose*`, deployment manifests.
- Build entrypoints: `Makefile`, task runners, scripts directories, CI workflows, Git hooks.
- Documentation: `README*`, `docs/`, `doc/`, ADRs, architecture diagrams, contribution guides.
- Agent knowledge: `AGENTS.md`, `AGENTS.override.md`, related instruction files, `.codex/skills`, `.agents/skills`, skill registries or indexes.
- Generated content: `bin/`, `obj/`, `dist/`, `build/`, coverage output, generated clients, code-generation configuration.
- Data changes: migration folders, schema tools, database projects, seed or deployment scripts.
- Sensitive configuration: `.env*`, user-secrets references, certificates, key files, credential templates. Record existence and handling; never copy secret contents.

## Command confirmation

Treat a command as confirmed only when an authoritative repository source defines it or current tooling proves it. Record:

| Scope | Purpose | Exact command | Evidence | Cost or side effects |
|---|---|---|---|---|
| Example area | Build, test, lint, typecheck, format, or run | Repository command | Manifest, script, CI, or docs path | Targeted/full; short/long-running; local/external |

Do not assume familiar defaults such as `npm test`, `dotnet test`, or `ng lint` exist merely because the technology is present.

## Architecture evidence

Look for stable boundaries rather than implementation recipes:

- allowed and forbidden project references;
- shared-to-feature or layer dependency direction;
- ownership of contracts and integration seams;
- isolation between deployable units;
- generated/source boundaries;
- module rules stated in architecture docs and consistently reflected by project references.

Label each finding `PROVEN`, `INFERRED`, or `UNRESOLVED`. Do not turn a single implementation example into an invariant without corroboration.

## Inspection limits

Start with files directly relevant to repository operation. Expand when required to:

- prove an architectural boundary;
- find a real verification command;
- understand an instruction file's scope;
- locate reusable project skills;
- distinguish source from generated output;
- resolve a contradiction.

Do not inspect unrelated business logic, enumerate every file, or read generated/build outputs simply to appear comprehensive.
