---
name: api-request-pipeline
description: Maintain ASP.NET Core middleware ordering, centralized exception mapping, request diagnostics, CORS, HTTPS, authentication, authorization, and OpenAPI pipeline behavior. Use when changing Program.cs, middleware, error responses, or cross-cutting HTTP processing.
---


## Scope

Limit this skill to cross-cutting ASP.NET Core request processing.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: src/Api/Program.cs; middleware registration; relevant middleware; current error contracts.

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

Compose with: http-contracts; api-authorization; backend-composition.

## Repository references

Start with: src/Api/Program.cs; middleware registration; relevant middleware; current error contracts.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core 10.0.10.

**Technical validation:** 2026-07.
# API Request Pipeline

Change cross-cutting HTTP behavior centrally and preserve security-sensitive middleware ordering.

## Workflow

1. Inspect src/Api/Program.cs, SeedWork/Extensions/MiddlewareRegister.cs, the relevant middleware, and current error contracts.
2. Classify the concern as exception handling, diagnostics, routing, CORS, authentication, authorization, HTTPS, OpenAPI, or environment-only startup.
3. Prefer one central implementation over endpoint-level repetition.
4. Map known exceptions to safe, stable HTTP responses. Log server-side details; never return stack traces, secrets, tokens, connection values, or raw unexpected exception messages.
5. Preserve ordering: exception handling early; authentication before authorization; endpoint mapping after security middleware.
6. Keep development-only OpenAPI/CORS behavior explicitly environment-scoped.
7. Treat changes to response envelopes as HTTP contract changes and compose with http-contracts.

## Repository constraints

- UseCustomMiddleware currently installs ExceptionMiddleware.
- Development startup calls InitializeDatabaseAsync; never run the API merely to verify pipeline changes.
- Request/response body logging is disabled. Do not enable it without reviewing credentials, tokens, personal data, file bodies, buffering cost, and redaction.
- Do not treat existing custom middleware as current framework best practice automatically. A migration to built-in exception handling or Problem Details is an architectural/contract change, not incidental cleanup.

## Error mapping

Handle validation, not-found, unauthenticated, forbidden, conflict, and unexpected failures consistently. Preserve status codes and response shapes unless the task authorizes a contract change. Use cancellation-aware response writes where supported.

## Verify

Build src/Api/Api.csproj. Inspect the final middleware order and exercise logic through focused tests where available; do not start the API or database. Confirm safe production responses and environment gates.

## Version handling

Adapted from dotnet-webapi and validated in 2026-07 for ASP.NET Core 10. Consult Context7 for newer IExceptionHandler, Problem Details, middleware diagnostics, or OpenAPI behavior before modernizing the established pipeline.
