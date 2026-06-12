# Core Guidance

## Shared Layout
- `src/src.sln`: primary backend solution.
- `src/Infrastructure/Infrastructure.sln`: infrastructure-focused solution.
- `src/Api/`, `src/Application/`, `src/Domain/`, `src/Infrastructure/`: backend layers.
- `src/ClientApp/`: Expo client.

## Shared Conventions
- C#: file-scoped namespaces, nullable reference types, and implicit usings are enabled.
- Use `PascalCase` for types and public members, `camelCase` for locals and fields.
- Keep edits narrow and consistent with nearby code.
- Prefer existing folder patterns over introducing new ones.
- Keep reads targeted: open only files directly needed to answer the task.
- Prefer the smallest relevant context set before expanding to nearby files or docs.
- While planning, if anything is not 100% certain, ask the user before proceeding.
- If a request is ambiguous or could be misunderstood, stop and ask for clarification before acting.
- Do not assume user intent when details are unclear; double-check user-provided facts and call out likely mistakes or better options.

## Commit Style
- Use short imperative commits with prefixes such as `feat:`, `fix:`, and `refactor:`.
- Keep each commit focused on one change.
