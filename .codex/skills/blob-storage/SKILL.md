---
name: blob-storage
description: Implement Azure Blob Storage upload, download, delete, content-type, identifier, stream, and compensation workflows behind Application ports. Use when changing BlobServiceClient registration, blob adapters, media/file storage, containers, or storage error handling.
---


## Scope

Limit this skill to Azure Blob Storage adapter workflows.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: storage port; BlobServiceClient registration; container model; neighboring blob adapter.

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

Compose with: application-ports; file-uploads; options-configuration; ef-writes-transactions.

## Repository references

Start with: storage port; BlobServiceClient registration; container model; neighboring blob adapter.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

Azure.Storage.Blobs 12.29.1; .NET 10.

**Technical validation:** 2026-07.
# Blob Storage

Keep Azure SDK types in Infrastructure and expose capability-focused Application ports.

## Workflow

1. Inspect the relevant port, BlobServiceClient registration, BlobContainerName, and one matching adapter.
2. Define container ownership, blob naming, size/type requirements, overwrite policy, metadata, authorization, and missing-blob semantics.
3. Reuse the singleton BlobServiceClient; create container/blob clients per operation.
4. Generate opaque identifiers rather than using client filenames or paths.
5. Stream data when possible, preserve stream ownership, reset seekable streams only when required, and forward CancellationToken.
6. Set trusted content type/headers from validated content, not blindly from request metadata.
7. Make delete semantics explicit and use DeleteIfExists only when idempotent success matches the contract.
8. Translate Azure failures at the adapter boundary only when Application has a meaningful outcome.
9. Coordinate database metadata and blob side effects with explicit ordering or compensation.

## Reliability and security

- Do not create containers implicitly unless lifecycle policy requires it.
- Do not expose connection strings, SAS tokens, account names, raw SDK exceptions, or internal container names.
- Avoid buffering whole files when bounded streaming can meet transformation needs; thumbnail generation may require bounded buffering and limits.
- Retries must be safe for the operation and naming policy.
- Dispose downloaded response streams according to the port's ownership contract.
- Validate image/media content before storage; compose with file-uploads.

## Verify

Build Infrastructure and focused tests with mocks/fakes where practical. Do not start Azurite, connect to Azure, create containers, or mutate blobs without explicit approval.

## Version handling

Validated in 2026-07 against Azure.Storage.Blobs 12.29.1 and .NET 10. Consult Context7 for newer SDK transfer options, retry behavior, stream ownership, conditional operations, or authentication APIs.
