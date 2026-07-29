---
name: ef-schema-changes
description: Assess and prepare EF Core model changes that may require migrations while enforcing repository approval boundaries. Use when entity configuration changes affect tables, columns, keys, relationships, indexes, sequences, conversions, or the model snapshot.
---


## Scope

Limit this skill to EF Core model deltas, migrations, and database approval boundaries.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: src/Infrastructure/AGENTS.md; entity/configuration; DbContext; project and migration state read-only.

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

Compose with: ef-modeling; ef-writes-transactions.

## Repository references

Start with: src/Infrastructure/AGENTS.md; entity/configuration; DbContext; project and migration state read-only.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET SDK 10.0.302; EF Core SQL Server/Design/Tools 10.0.10.

**Technical validation:** 2026-07.
# EF Schema Changes

Separate authored model changes from generated migration artifacts and database application.

## Approval boundaries

- Domain/entity and IEntityTypeConfiguration changes requested by the user may be implemented normally.
- Creating, deleting, renaming, or editing a migration or ApplicationDbContextModelSnapshot requires an explicit user request.
- Applying a migration, running database update, starting the development API, or otherwise mutating a database requires separate explicit approval.
- Infrastructure/Data/Migrations is locally present but ignored by Git. Never assume it may be regenerated, cleaned, or discarded.

## Workflow

1. Inspect src/Infrastructure/AGENTS.md, the entity, configuration, ApplicationDbContext, project/SDK versions, migrations, and snapshot read-only.
2. Describe the intended model delta: schema object, type/nullability, default/backfill, index/key, relationship/delete behavior, conversion, or sequence.
3. Classify data risk: additive, backfill required, narrowing, rename, split/merge, destructive, or provider-specific.
4. Align domain behavior, contracts, validation, and EF mapping.
5. If migration generation was not explicitly requested, stop after model changes and report that a migration remains required.
6. If generation was requested, use the repository-evidenced dotnet ef command and correct startup/target projects; review every generated operation.
7. If application was separately approved, verify the exact target database/environment before executing.

## Review generated operations

Check provider types, nullability transitions, defaults, data backfills, constraint/index names, foreign keys, delete behavior, sequences, renames versus drop/add, and Down reversibility. Never hand-wave possible data loss.

## Verification

Without database approval, limit verification to builds, tests, and artifact inspection. Do not start Docker, SQL Server, or the API. Report migration generation/application separately and precisely.

## Version handling

Validated in 2026-07 against EF Core SQL Server/Design/Tools 10.0.10 and .NET SDK 10.0.302. Always revalidate dotnet ef behavior with Context7 when the SDK or EF tools version changes.
