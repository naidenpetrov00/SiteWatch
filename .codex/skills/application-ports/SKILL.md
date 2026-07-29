---
name: application-ports
description: Design Application-layer interfaces for persistence, identity, storage, email, devices, invoices, and other external capabilities, then implement adapters in Infrastructure. Use when adding or changing an external dependency boundary or service interface.
---


## Scope

Limit this skill to Application-owned external capability interfaces and Infrastructure adapters.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: concrete use case; existing Application interfaces; candidate adapter; DI registration.

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

Compose with: mediator-use-cases; backend-composition; ef-writes-transactions; blob-storage; email-delivery.

## Repository references

Start with: concrete use case; existing Application interfaces; candidate adapter; DI registration.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10; current Application/Infrastructure architecture.

**Technical validation:** 2026-07.
# Application Ports

Define the capability Application needs, not the API shape of the vendor that provides it.

## Workflow

1. Start from a concrete use case and identify the external capability, inputs, outputs, failure modes, cancellation, and idempotency needs.
2. Search src/Application/SeedWork/Interfaces for a suitable existing port before adding one.
3. Place the interface in Application with transport-neutral types; keep vendor SDK, HttpContext, EF concrete context, SMTP, Azure, and generated client types out.
4. Make methods asynchronous only for real I/O and accept CancellationToken for cancellable operations.
5. Return the smallest useful result. Model expected failures explicitly when callers can act on them.
6. Implement the adapter in Infrastructure near the integration and register it in backend-composition.
7. Test Application orchestration against a fake/stub and test adapter-specific behavior separately when practical.

## Design rules

- Prefer capability-focused ports over one broad service or one interface per vendor method.
- Do not create generic IRepository wrappers over IApplicationDbContext without demonstrated value.
- Keep streams and disposable ownership explicit; document whether the caller or callee disposes returned resources.
- Avoid leaking IdentityResult, BlobClient, MailMessage, generated ONVIF clients, or EF transactions across the boundary.
- Keep camera/device and invoice/persistence workflows behind ordinary ports. Add a dedicated skill only if their engineering decisions evolve independently.
- Changing an existing interface requires reviewing every adapter, handler, registration, and test double.

## Failure and consistency

Define timeout, retry, duplicate, not-found, and partial-success semantics at the boundary. Retries require idempotency analysis. Database transactions do not cover external calls; coordinate with ef-writes-transactions.

## Verify

Build Application and Infrastructure, inspect dependency direction, and run focused tests. Do not call external systems or infrastructure without explicit approval.

## Version handling

Validated in 2026-07 against the current .NET 10 Application/Infrastructure split. Port design is version-agnostic; use Context7 only for vendor-specific behavior inside an adapter or when a newer SDK changes cancellation, disposal, or result semantics.
