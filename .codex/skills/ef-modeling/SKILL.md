---
name: ef-modeling
description: Configure EF Core entity persistence, relationships, keys, indexes, precision, owned value objects, conversions, comparers, sequences, and Identity model integration. Use when changing ApplicationDbContext or IEntityTypeConfiguration classes under Infrastructure.
---


## Scope

Limit this skill to EF Core model and entity configuration.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: entity/value object; IApplicationDbContext; ApplicationDbContext; neighboring configurations; migration state read-only.

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

Compose with: domain-entities; value-objects; ef-schema-changes; ef-queries; ef-writes-transactions.

## Repository references

Start with: entity/value object; IApplicationDbContext; ApplicationDbContext; neighboring configurations; migration state read-only.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

EF Core SQL Server 10.0.10; ASP.NET Core Identity EF Core 10.0.10.

**Technical validation:** 2026-07.
# EF Core Modeling

Keep persistence mapping in Infrastructure and make database constraints reinforce domain and application rules.

## Workflow

1. Inspect the entity/value object, IApplicationDbContext, ApplicationDbContext, neighboring configurations, and current migrations/snapshot read-only when relevant.
2. Add or revise one IEntityTypeConfiguration<T> per entity under Infrastructure/Data/Configurations.
3. Configure requiredness, lengths, precision, conversions, generated values, table/column names, indexes, relationships, delete behavior, and concurrency deliberately.
4. Keep ApplicationDbContext focused on DbSets, configuration discovery, Identity integration, and save behavior.
5. Use owned types for cohesive value objects mapped to multiple columns.
6. Use a ValueConverter plus ValueComparer when a single-column mutable/reference value needs stable change tracking and snapshots.
7. Align domain validation, request validation, and database constraints without treating any one layer as a substitute for the others.
8. Assess whether the model change requires a migration and activate ef-schema-changes.

## Repository patterns

- Configurations are discovered with ApplyConfigurationsFromAssembly.
- SQL Server sequences generate NumberId values for applicable entities.
- ApplicationDbContext derives from IdentityDbContext<ApplicationUser> and implements IApplicationDbContext.
- Application exposes DbSet-based access through IApplicationDbContext; do not move the concrete DbContext inward.
- Cascade, restrict, and many-to-many behavior must be chosen from ownership and deletion semantics, not copied blindly.

## Guardrails

- Do not put EF attributes or Infrastructure references into Domain as an incidental shortcut.
- Do not manually edit generated migrations or the model snapshot without explicit authorization.
- Do not rely on CLR defaults for monetary precision, required strings, or destructive delete behavior.
- Add indexes for demonstrated lookup/sort/uniqueness needs; remember indexes impose write/storage cost.
- Treat converter storage formats as persisted contracts.

## Verify

Build Infrastructure and inspect model consistency through non-mutating checks. Do not start the API, create/apply migrations, or connect to a database unless separately authorized.

## Version handling

Validated in 2026-07 against EF Core SQL Server and Identity EF Core 10.0.10. Consult Context7 for newer mapping, owned/complex type, value generation, converter, comparer, or relationship behavior.
