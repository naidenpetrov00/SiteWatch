---
name: dto-mapping
description: Create and maintain Application DTO mappings and EF-translatable projections with AutoMapper or explicit mapping. Use when adding DTOs, nested Profile mappings, ProjectTo queries, computed output fields, or diagnosing mapping/translation failures.
---


## Scope

Limit this skill to DTO mapping and EF-translatable projection.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: source entity/value object; target DTO; mapping profile; consuming query.

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

Compose with: http-contracts; ef-queries; table-queries.

## Repository references

Start with: source entity/value object; target DTO; mapping profile; consuming query.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

AutoMapper 16.2.0; EF Core 10.0.10.

**Technical validation:** 2026-07.
# DTO Mapping

Choose mapping based on where data lives: project database queries before materialization; map domain objects explicitly when behavior or translation makes projection unsuitable.

## Workflow

1. Inspect the source entity/value object, target DTO, neighboring mappings, and the query that consumes the mapping.
2. Define DTO nullability and collection defaults deliberately; never expose entities, credentials, or navigation graphs.
3. For EF reads, prefer ProjectTo<TDto>(mapper.ConfigurationProvider) when every expression translates and it avoids unnecessary columns.
4. Put a small nested Profile class beside the DTO when that is the local pattern.
5. Use ForMember for value-object scalar values, enum text, computed display fields, and differently named members.
6. Use explicit From or Select mapping when translation is uncertain, a domain method is required, or the table-query framework already materializes a known shape.
7. Project last in the LINQ pipeline, after entity filters and ordering.
8. Add a focused configuration or query test when the mapping is non-trivial.

## Translation rules

- Do not use arbitrary instance methods, culture-dependent formatting, or non-translatable domain properties inside ProjectTo.
- Do not combine Include with a DTO projection merely to populate projected members.
- Avoid materializing an entity graph before mapping unless the use case needs domain behavior.
- Keep API-only serialization concerns out of Application profiles.
- Treat credentials and secret-bearing entity fields as denied by default, even when AutoMapper could map them by convention.

## Compose with

Use http-contracts for public shape, ef-queries for query construction, and table-queries for table-specific projections.

## Verify

Build src/Application/Application.csproj. Validate the AutoMapper configuration when a test harness exists and inspect generated SQL or translation behavior through non-sensitive diagnostics when performance or translation is uncertain. Do not enable sensitive-data logging.

## Version handling

Validated in 2026-07 against AutoMapper 16.2.0 and EF Core 10.0.10. Consult Context7 when ProjectTo, profile discovery, expression translation, or licensing/configuration behavior changes in newer versions.
