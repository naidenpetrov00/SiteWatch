---
name: table-queries
description: Implement dashboard-style EF Core table queries using the repository's reusable filtering, sorting, counting, and pagination definitions. Use when adding TableQueryRequest queries, TableQueryDefinition descriptors, table DTOs, or paged dashboard endpoints.
---


## Scope

Limit this skill to allow-listed table filtering, sorting, counting, and pagination.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: Application.SeedWork.Queries; a neighboring partial table query; target entity/configuration.

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

Compose with: ef-queries; validation-pipeline; dto-mapping; minimal-api-endpoints.

## Repository references

Start with: Application.SeedWork.Queries; a neighboring partial table query; target entity/configuration.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

EF Core 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.
# Table Queries

Use the shared Application.SeedWork.Queries framework rather than rebuilding filtering, sorting, and pagination per feature.

## Workflow

1. Inspect TableQueryRequest, TableQueryDefinition, TableQueryEvaluator, filter predicates, and a nearby partial query with a .Table.cs definition.
2. Define request filter properties on a sealed partial query that inherits TableQueryRequest and implements IRequest<PagedResult<TDto>>.
3. Define one static TableQueryDefinition in a companion partial file.
4. Add only allow-listed filter descriptors and sort keys; never accept arbitrary property names or raw expressions.
5. Give every sort a deterministic tie-breaker, normally the entity ID.
6. Provide a deterministic default sort.
7. Start from the narrowest entity query, use AsNoTracking, and call ToPagedResultAsync with a translatable projection.
8. Map the resulting items to the public table DTO only once.
9. Validate PageIndex, PageSize, filter formats, and allowed bounds with FluentValidation.

## Query semantics

- Preserve zero-based PageIndex and the established default PageSize unless the contract intentionally changes.
- Keep TotalCount as the unfiltered count and FilteredCount as the post-filter count.
- Apply filters before sorting and paging.
- Normalize keys case-insensitively through the existing dictionaries; do not silently make values culture-sensitive.
- Bound page size to prevent unbounded reads.
- Keep computed filters and sorts SQL-translatable; inspect generated SQL when a complex expression is added.
- For search columns that are persisted and indexed, use those normalized columns instead of recomputing expensive text at query time.

## Compose with

Use ef-queries for performance, validation-pipeline for request bounds, dto-mapping for projections, and minimal-api-endpoints with [AsParameters] for binding.

## Verify

Add focused tests for no filters, each new filter, ascending/descending sorts, unknown sort fallback, stable paging, empty pages, and counts. Build Application; do not connect to a database without explicit approval.

## Version handling

Validated in 2026-07 against EF Core 10.0.10 and the repository's current table-query framework. The local framework is authoritative; consult Context7 only for version-sensitive LINQ translation or EF performance behavior.
