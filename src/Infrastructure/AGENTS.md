# Infrastructure Instructions

These rules supplement the repository root instructions for `src/Infrastructure`.

## Boundaries

- This project owns EF Core persistence and implementations for identity, storage, email, cameras, invoices, persons, and sites.
- Keep abstractions needed by application use cases in `Application`; implement them here. Do not move infrastructure dependencies into `Application` or `Domain` as an incidental shortcut.
- Existing option types use `GetOptions<T>` and derive section names from the `Options` suffix. Preserve that contract unless the task explicitly coordinates a configuration redesign.

## Data and migrations

- Model and EF configuration changes needed by the requested behavior are allowed.
- Creating, deleting, or editing migrations and `ApplicationDbContextModelSnapshot` requires an explicit user request. `src/Infrastructure/Data/Migrations` is locally present but ignored by Git; do not assume it may be regenerated or discarded.
- Applying migrations or running any command that changes a database requires explicit approval. Development API startup also applies migrations and seeds data, so it is covered by this restriction.
- Treat `sqlserver` and `azurelite-data` as runtime data, not authored source. Do not edit, delete, replace, or commit new runtime files there as part of normal implementation.

## Generated service clients

- Treat `Cameras/Services/Onvif/Generated` and its `*.g.cs` files as generated output. Do not hand-edit them.
- No regeneration command is documented in the repository. Change generator inputs or regenerate clients only when the user explicitly requests it, and record the command used.

## External operations

- For an explicitly requested database or external-device operation, state the expected side effects before requesting approval.
