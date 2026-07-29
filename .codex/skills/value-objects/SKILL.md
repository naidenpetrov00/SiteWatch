---
name: value-objects
description: Create and evolve domain value objects with normalized construction, structural equality, invariant enforcement, and EF persistence compatibility. Use when modeling a concept by value rather than identity or changing a type under src/Domain/ValueObjects.
---


## Scope

Limit this skill to domain value objects and structural equality.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: Domain.SeedWork.ValueObject; neighboring value objects; consumer entity; EF mapping.

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

Compose with: domain-entities; ef-modeling; backend-testing.

## Repository references

Start with: Domain.SeedWork.ValueObject; neighboring value objects; consumer entity; EF mapping.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10; Ardalis.GuardClauses 5.0.0; EF Core 10.0.10.

**Technical validation:** 2026-07.
# Value Objects

Use a value object when a domain concept has meaningful validation/equality and no independent identity.

## Workflow

1. Inspect Domain.SeedWork.ValueObject, neighboring value objects, consuming entities, JSON needs, and EF configuration.
2. Define the complete invariant and canonical representation.
3. Validate and normalize at construction; expose a named factory or established implicit conversion only when its behavior is unsurprising.
4. Keep value objects immutable by default. Treat mutation of an existing value object as a compatibility decision, not a pattern to copy.
5. Yield every equality component in stable order from GetEqualityComponents; normalize unordered collections before storing them.
6. Provide ToString only when a single scalar representation is unambiguous.
7. Configure persistence with an owned type or value converter/comparer in Infrastructure.
8. Test valid creation, normalization, boundaries, equality, inequality, hash consistency, round trips, and invalid input.

## Guardrails

- Do not use a value object for an independently addressable lifecycle.
- Do not expose invalid intermediate state through public setters.
- Do not let serializer requirements weaken domain invariants.
- Keep serialization/storage helpers deterministic and backward-compatible when persisted values already exist.
- If using implicit conversions, avoid ambiguous, lossy, or exception-surprising conversions.
- Match domain limits and EF column limits; correct mismatches deliberately rather than silently.

## Compose with

Use domain-entities for aggregate behavior, ef-modeling for owned types/converters/comparers, and backend-testing for equality and round-trip coverage.

## Verify

Build Domain and Infrastructure when persistence mapping changes. Run focused tests without a database where possible. Confirm equal objects have equal hashes and storage conversion preserves all equality components.

## Version handling

Validated in 2026-07 against .NET 10, Ardalis.GuardClauses 5.0.0, and EF Core 10 value conversion/owned-type patterns. Core modeling is version-agnostic; consult Context7 for newer EF materialization, converter, or comparer behavior.
