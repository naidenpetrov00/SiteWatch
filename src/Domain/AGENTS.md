# Domain layer instructions

## Scope and ownership

- This file applies to `src/Domain`.
- Domain is the innermost backend layer. Its project has no project references and must not reference Application, Infrastructure, or Api.
- Keep domain state, entities, value objects, and business invariants here when they are independent of delivery and persistence concerns.
- Keep HTTP, database access, environment/configuration access, storage, device integration, and host composition out of this layer.

## Task guidance

- Inspect the relevant entity, value object, and nearby seed-work abstractions before changing a domain concept.
- When a domain change affects a use case, persistence mapping, endpoint contract, or client contract, load only the corresponding scoped AGENTS files and limit edits to what the request requires.
- Use applicable .NET skills for concrete implementation guidance. Do not encode framework syntax or tutorials in this file.
