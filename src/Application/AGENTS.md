# Application layer instructions

## Scope and ownership

- This file applies to `src/Application`.
- Application depends on Domain and must not reference Infrastructure or Api.
- Keep use cases, commands, queries, validation, application-facing abstractions, mapping, authorization behavior, and pipeline behavior in this layer.
- Keep concrete persistence, storage, email, camera/device, and transport implementations in Infrastructure. Keep HTTP endpoint and host concerns in Api.

## Task guidance

- Follow the existing feature ownership under areas such as Cameras, Identity, Invoices, Persons, and Sites before introducing a new location.
- Prefer an existing application abstraction or local pattern over a parallel abstraction. Introduce shared abstractions only for a concrete ownership or reuse need.
- When a contract change crosses into Domain, Infrastructure, Api, Dashboard, or ClientApp, read only the affected scopes and preserve the dependency direction.
- Use applicable .NET, ASP.NET Core, or data skills for concrete implementation guidance.
