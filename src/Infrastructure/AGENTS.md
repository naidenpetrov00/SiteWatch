# Infrastructure layer instructions

## Scope and ownership

- This file applies to `src/Infrastructure`.
- Infrastructure depends on Application and supplies concrete persistence and external-system implementations for application abstractions.
- EF Core data access and configuration, Identity, storage, email, invoices, site services, and camera/ONVIF integrations belong here when they implement an application-owned boundary.
- Do not move business policy into infrastructure services merely because they coordinate persistence or an external system.

## Data and generated boundaries

- Treat model and EF configuration edits separately from migrations. A requested model/configuration change does not authorize migration generation, migration editing, snapshot changes, or applying a database change. Leave migration generation to the user; never hand-edit migration files or `ApplicationDbContextModelSnapshot.cs`, and never apply EF migrations manually or by command.
- Never execute a database mutation without explicit approval for the named environment.
- Treat files under generated-looking paths, including `Cameras/Services/Onvif/Generated`, as generator-owned until their source and regeneration process are identified. Do not hand-edit or regenerate them without authorization.
- Keep secrets and connection values out of source and agent output. Do not inspect or echo secret values merely to understand configuration.

## Task guidance

- Use applicable EF Core, ASP.NET Core, or .NET integration skills for implementation details. Keep this file focused on boundaries.
- If an infrastructure change alters an application abstraction, domain model, API contract, or client behavior, load only those additional scopes that are actually affected.
