---
name: options-configuration
description: Add and maintain typed backend configuration for database, JWT, blob storage, email, and external integrations without exposing secrets. Use when changing appsettings sections, environment variables, option classes, binding, validation, or configuration consumption.
---


## Scope

Limit this skill to typed backend configuration and secret-safe binding.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: option class; appsettings section names; custom GetOptions binder; registration and consumer.

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

Compose with: backend-composition; identity-integration; blob-storage; email-delivery.

## Repository references

Start with: option class; appsettings section names; custom GetOptions binder; registration and consumer.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

Microsoft.Extensions.Configuration/DependencyInjection 10.0.10.

**Technical validation:** 2026-07.
# Options Configuration

Treat configuration names and required values as runtime contracts; treat secret values as sensitive data.

## Workflow

1. Inspect the option class, appsettings section names, environment overlays, current GetOptions extension, DI registration, and consuming service without printing secret values.
2. Separate non-secret defaults from secrets and environment-specific values.
3. Use a focused option type with nullable properties only when absence is valid.
4. Bind one explicit section and validate required fields, formats, ranges, and cross-field rules before first use.
5. Prefer constructor-injected typed options for newly standardized code, but do not migrate the repository's custom GetOptions convention incidentally.
6. Keep environment-variable names documented in safe configuration/docs, never their values.
7. Update deployment/local setup documentation when required configuration changes.

## Current convention

GetOptions<TOptions> derives the section name by removing the Options suffix. Preserve that behavior for compatible changes. A move to IOptions<T>, ValidateDataAnnotations, ValidateOnStart, or explicit section constants is a broader consistency change and must update all affected registrations/consumers together.

## Security rules

- Never read configuration files or environment variables in a way that echoes secrets into logs or responses.
- Never commit connection strings, JWT signing keys, SMTP passwords, tokens, or certificates.
- Do not use a fallback secret or silently continue with an empty value.
- Validate ports, durations, URIs, issuer/audience, and algorithms with appropriate types rather than free-form strings where a compatible change permits.
- Do not cache mutable configuration accidentally; choose IOptions, IOptionsSnapshot, or IOptionsMonitor based on intended reload behavior.

## Verify

Build affected projects and test binding with synthetic non-secret values when practical. Do not connect to services. Confirm missing/invalid required settings fail with a useful message that does not reveal values.

## Version handling

Validated in 2026-07 against Microsoft.Extensions.Configuration/DI 10.0.10 and the current custom binder. Consult Context7 for newer options validation, source generation, reload, or configuration binding behavior before modernizing.
