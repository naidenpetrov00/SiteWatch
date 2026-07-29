---
name: dashboard-models-mapping
description: Define and evolve strict Dashboard API models, form-to-request mappers, response normalization, formatters, calculations, and serialized contract boundaries. Use when changing feature model files, request or response shapes, mapper utilities, date or number conversion, discriminated payloads, lookup models, or pure data transformations under src/Dashboard.
---

# Dashboard Models and Mapping

Keep serialized API contracts explicit and keep transformation logic out of components.

## Scope and required context: confirm the boundary

1. Inspect the relevant Dashboard model, feature service, mapper, component, and tests.
2. Inspect the corresponding API endpoint or Application DTO read-only when the wire shape is uncertain.
3. Distinguish serialized data from form state and view-only state.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: contracts

- Use dedicated TypeScript interfaces or type aliases for request, response, table row, lookup, and detail shapes.
- Match JSON field names, nullability, optionality, arrays, identifiers, and date representations exactly.
- Use readonly arrays for response collections that consumers should not mutate.
- Do not expose form controls, Material types, TanStack result types, or backend implementation entities as serialized models.
- Use narrow string unions or discriminants when the API contract proves the allowed values.
- Do not use any to bypass uncertain contracts; resolve the contract or mark the uncertainty.

## Map at feature boundaries

- Put feature request mappers and pure formatters under the owning feature's utils directory.
- Convert FormGroup raw values into request models in a pure mapper.
- Trim, normalize, omit blank repeatable rows, and convert numeric or date values deliberately.
- Preserve local calendar dates without accidental UTC conversion when the API expects date-only strings.
- Treat optional omission and explicit null as different wire behaviors.
- Do not mutate the form value, source DTO, or cached query data during mapping.
- Keep calculations deterministic and free of Angular injection so they can be tested directly.

## Evolve safely

- Search all consumers before renaming or changing a serialized field.
- Update request models, response models, mappers, services, templates, and tests together.
- Coordinate pagination and table contract changes with dashboard-data-tables and dashboard-data-access.
- Coordinate locale-sensitive user display with dashboard-internationalization; do not localize serialized values.

## Related skills and repository references

Compose with `dashboard-data-access`, `dashboard-forms-dialogs`, `dashboard-data-tables`, and `dashboard-testing`. Inspect the matching backend contract and existing feature models as repository references.

## Verification and definition of done

- Add focused tests for normal, empty, boundary, optional, discriminator, date, and numeric cases.
- Confirm the service generic type matches the actual endpoint result.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for TypeScript 5.9.3 and the current SiteWatch Dashboard/API contracts. The repository is authoritative for wire shapes. Query Context7 only for an unfamiliar serialization API or version-sensitive Angular form integration.
