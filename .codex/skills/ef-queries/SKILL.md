---
name: ef-queries
description: Write and optimize EF Core read queries with correct tracking, projection, translation, paging, related-data loading, and cancellation. Use for slow queries, N+1 risks, excessive materialization, ProjectTo, AsNoTracking, or new Application read handlers.
---


## Scope

Limit this skill to EF Core read-query design and performance.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: use case; entity configuration/indexes; DTO mapping; query cardinality.

## Architecture

Preserve Api -> Infrastructure -> Application -> Domain and keep transport, use-case, domain, persistence, and external-adapter concerns in their owning layers.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: dto-mapping; table-queries; ef-modeling.

## Repository references

Start with: use case; entity configuration/indexes; DTO mapping; query cardinality.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

EF Core 10.0.10; AutoMapper 16.2.0.

**Technical validation:** 2026-07.
# EF Core Queries

Shape the query in SQL, retrieve only required data, and measure before introducing specialized optimizations.

## Workflow

1. Inspect the use case, entity configuration/indexes, DTO mapping, expected cardinality, and a representative neighboring query.
2. Start from IApplicationDbContext in Application unless the operation requires a concrete Infrastructure-only capability.
3. Apply filters before ordering, projection, and materialization.
4. Use AsNoTracking for read-only entity queries; use tracking only when the same context will update the entity.
5. Project directly to the required DTO with Select or ProjectTo when the expression is translatable.
6. Use AnyAsync for existence, SingleOrDefaultAsync for uniqueness assumptions, and FirstOrDefaultAsync only when first-match semantics are intentional.
7. Forward CancellationToken to every async EF operation.
8. Bound collections with paging or an explicit limit and use deterministic ordering.
9. Inspect generated SQL/query count with safe diagnostics when performance is part of the task.

## Decision rules

- Prefer projection over Include when returning DTOs.
- Use Include for entity graphs needed for domain behavior; consider AsSplitQuery only after evaluating cartesian growth and consistency needs.
- Avoid N+1 loops, early ToList, client-side filtering, and per-row service calls.
- Use ExecuteUpdateAsync/ExecuteDeleteAsync only for true set-based writes and compose with ef-writes-transactions.
- Consider compiled queries only for measured hot paths; do not make them a default.
- Use parameterized LINQ or FromSqlInterpolated; never concatenate user input into SQL.
- Never enable sensitive-data logging in shared configuration or expose parameter values.

## Repository composition

Use table-queries for dashboard filtering/sorting/paging and dto-mapping for AutoMapper projection. Domain exceptions establish not-found behavior; endpoints should not duplicate the lookup.

## Verify

Build Application and Infrastructure. Add focused query tests when practical. For performance work, compare SQL shape, round trips, rows/columns, allocation, and indexes; database execution requires explicit approval.

## Version handling

Adapted from optimizing-ef-core-queries and validated in 2026-07 against EF Core 10.0.10 and AutoMapper 16.2.0. Query Context7 for version-sensitive translation, split-query, compiled-query, or bulk-operation behavior.
