---
name: email-delivery
description: Implement secure transactional email workflows behind Application ports, including SMTP configuration, verification/reset templates, token links, disposal, failure behavior, and side-effect coordination. Use when changing IEmailService, EmailService, EmailTemplates, or identity email workflows.
---


## Scope

Limit this skill to transactional email adapter workflows.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: IEmailService; EmailService; templates; GmailOptions; calling identity workflow.

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

Compose with: identity-integration; options-configuration; application-ports; ef-writes-transactions.

## Repository references

Start with: IEmailService; EmailService; templates; GmailOptions; calling identity workflow.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10 System.Net.Mail; ASP.NET Core Identity 10.0.10.

**Technical validation:** 2026-07.
# Email Delivery

Treat email as an external side effect with sensitive content, uncertain delivery, and retry/duplication risks.

## Workflow

1. Inspect IEmailService, EmailService, templates, GmailOptions, identity command, and token-generation flow.
2. Define recipient, purpose, safe subject/body, token/link encoding, expiry expectations, sender identity, and expected failure behavior.
3. Keep SMTP and MailMessage types in Infrastructure; keep Application methods use-case-oriented.
4. Load credentials from protected runtime configuration/environment and validate non-secret host/port/sender settings.
5. Encode user-controlled content and tokens correctly for the message format and URL context.
6. Dispose MailMessage and SMTP resources, propagate cancellation where the selected API supports it, and avoid blocking calls.
7. Decide whether failure should fail the use case, queue for retry, or be compensated; do not report delivery merely because submission succeeded.
8. Make retries idempotent enough to avoid uncontrolled duplicate mail.
9. Test template rendering and orchestration with synthetic recipients/tokens.

## Security and privacy

- Never log SMTP credentials, verification/reset tokens, full message bodies, or sensitive recipient data.
- Do not embed secrets in source, comments, appsettings, tests, or templates.
- Avoid account enumeration in externally visible responses.
- Use Identity's purpose-bound tokens; do not invent reset/verification token formats.
- Do not place raw exception messages from SMTP into HTTP responses.
- Keep links configurable per environment and require HTTPS outside local development.

## Compose with

Use identity-integration for token workflows, options-configuration for settings, application-ports for the boundary, and ef-writes-transactions when email follows a database state change.

## Verify

Build Application and Infrastructure and run non-network template/orchestration tests. Do not send email or connect to SMTP without explicit approval.

## Version handling

Validated in 2026-07 against .NET 10 System.Net.Mail and ASP.NET Core Identity 10.0.10. Because email security and transport behavior can change, consult current Context7/official guidance before provider, TLS, credential, or token-flow changes.
