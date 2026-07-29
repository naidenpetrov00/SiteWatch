---
name: file-uploads
description: Implement secure multipart uploads and streamed downloads in ASP.NET Core minimal APIs, including binding, validation, limits, antiforgery decisions, stream ownership, and handoff to storage ports. Use for IFormFile, multipart form data, media/file endpoints, or large-file streaming.
---


## Scope

Limit this skill to secure multipart upload and streamed download boundaries.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: upload endpoint; uploaded-file request; validator; storage port and adapter.

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

Compose with: minimal-api-endpoints; validation-pipeline; application-ports; blob-storage.

## Repository references

Start with: upload endpoint; uploaded-file request; validator; storage port and adapter.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core 10.0.10; Azure.Storage.Blobs 12.29.1.

**Technical validation:** 2026-07.
# File Uploads

Keep HTTP multipart handling in Api, validation/orchestration in Application, and storage in Infrastructure.

## Workflow

1. Inspect a neighboring upload endpoint, the Application uploaded-file type, validator, storage port, and adapter.
2. Define allowed size, actual file formats, metadata, authentication, storage destination, and compensation behavior before coding.
3. For mixed form fields, annotate every form-bound parameter with [FromForm] or use one bound form type.
4. Configure both request-body and multipart limits when requirements exceed defaults; choose the smallest justified limits.
5. Validate non-empty content, declared type, and actual file signature. Never trust FileName, extension, or ContentType alone.
6. Open the request stream with await using, pass a transport-neutral stream/content-type object to Application, and propagate cancellation.
7. Use generated storage identifiers. Never build a storage or filesystem path from a client filename.
8. Use IFormFile for bounded uploads; use MultipartReader or another streaming design for genuinely large content.
9. Return only safe identifiers and metadata.

## Antiforgery and authentication

The current JWT API upload endpoints use DisableAntiforgery. Keep that opt-out only when cookie authentication is not used for the request. Cookie-authenticated form endpoints require antiforgery protection. Protect media endpoints according to their data sensitivity rather than copying an existing public route.

## Consistency

Uploads that write a blob and then database metadata cross two systems. Define cleanup or compensation for partial failure; do not pretend an EF transaction covers blob storage. Deletes should be idempotent where the contract permits and must not remove metadata for the wrong blob.

## Compose with

Use minimal-api-endpoints, validation-pipeline, application-ports, blob-storage, and ef-writes-transactions.

## Verify

Build affected projects and add focused validation/orchestration tests. Do not upload to external storage or start Azurite without explicit approval. Review stream disposal, limits, signature checks, cancellation, authorization, and partial-failure handling.

## Version handling

Adapted from minimal-api-file-upload and validated in 2026-07 for ASP.NET Core 10. Consult Context7 when multipart binding, antiforgery, request limits, or streaming APIs differ in a newer framework version.
