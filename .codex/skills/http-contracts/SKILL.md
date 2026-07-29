---
name: http-contracts
description: Design and evolve backend HTTP request, response, file, and pagination contracts without exposing domain or persistence models. Use when adding DTOs, changing serialized fields, choosing status/result shapes, or assessing API compatibility.
---


## Scope

Limit this skill to HTTP request, response, pagination, and file contracts.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: the endpoint; related Application DTO/request; client consumers when compatibility is affected.

## Architecture

Preserve Api -> Infrastructure -> Application -> Domain and keep transport, use-case, domain, persistence, and external-adapter concerns in their owning layers.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Decision rules

Ask before an architecture change, production dependency, breaking contract, migration artifact, database/external connection, or materially broader behavior.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: minimal-api-endpoints; dto-mapping; table-queries; api-request-pipeline.

## Repository references

Start with: the endpoint; related Application DTO/request; client consumers when compatibility is affected.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core 10.0.10; AutoMapper 16.2.0.

**Technical validation:** 2026-07.
# HTTP Contracts

Treat public contracts as a transport boundary, not as aliases for domain entities.

## Workflow

1. Inspect the endpoint, its Application request/DTO, current client usage when relevant, and neighboring contracts.
2. Decide whether the change is additive, compatible, or breaking. Ask before intentionally breaking an existing client contract.
3. Keep transport-only binding concerns in Api; keep reusable use-case inputs and output DTOs in Application.
4. Use dedicated DTOs with deliberate nullability and stable serialized names. Prefer records and init-only properties when compatible with the local pattern.
5. Use DateTimeOffset for instants and offset-bearing dates unless the domain explicitly models another concept.
6. Represent paged tables with the established PagedResult<T> shape and table-query request fields.
7. Return file streams with an accurate content type and defined ownership/lifetime.
8. Update endpoint result types and documentation together.

## Contract rules

- Never serialize EF navigation graphs, Identity entities, or domain entities directly.
- Do not expose secrets, passwords, storage connection data, internal exception messages, or camera credentials.
- Distinguish omitted optional input from an explicit clear operation when implementing partial updates.
- Keep enum representation compatible with existing clients; do not globally change enum or JSON settings incidentally.
- Use stable identifiers and resource locations for create responses.
- Keep validation errors structured and field-addressable.
- Reuse Application DTOs only when they are genuinely transport-neutral and already serve that boundary.

## Compatibility checks

Search both frontend applications and .http examples when a field, route, status code, nullability rule, or enum representation changes. Document and test intentional compatibility changes. Do not introduce globally stricter JSON behavior during an unrelated endpoint change.

## Compose with

Use minimal-api-endpoints for route implementation, dto-mapping for projections, table-queries for paged contracts, and api-request-pipeline for error envelopes.

## Verify

Confirm the OpenAPI-inferred schema matches the intended shape, no entity type appears in the signature, nullable fields reflect behavior, and consumers/tests are updated when required. Build without starting the API.

## Version handling

Validated in 2026-07 against ASP.NET Core 10 and current Application DTO patterns. Contract principles are stable; consult Context7 only for newer serializer, OpenAPI, binding, or typed-result behavior.
