---
name: dashboard-async-ui-state
description: Design consistent loading, fetching, empty, error, retry, mutation-pending, and submission feedback for Angular Dashboard workflows. Use when rendering TanStack Query or mutation status, handling failed HTTP or dialog operations, disabling actions, preserving form state, or adding accessible progress and error messaging.
---

# Dashboard Async UI State

Represent each async phase deliberately instead of treating missing data as success.

## Scope and required context: model view states

- Distinguish initial pending from background fetching.
- Show empty state only after a successful result proves the collection is empty.
- Keep stale data visible during a background refetch when the query already has usable data.
- Show an actionable error state for a failed initial load and a less disruptive notice for a failed background refresh.
- Provide retry only when the operation is safe to repeat.
- Derive these states from TanStack Query or mutation signals; do not copy them into duplicate writable signals.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: mutations

- Disable only controls that would submit the same operation again.
- Preserve form and dialog state when a request fails.
- Close a dialog, navigate, or clear input only after confirmed success.
- Use the mutation's pending signal as the source of truth rather than a parallel boolean.
- Keep validation errors distinct from transport, authorization, conflict, and unexpected failures.
- Avoid empty catch blocks. If a boundary intentionally keeps the UI usable, surface or delegate the error and document why no local action is needed.

## Present feedback accessibly

- Use stable progress copy rather than rapidly flashing spinners for trivial background work.
- Mark the affected region busy when users need to know interaction is temporarily limited.
- Announce important asynchronous errors or completion messages without moving focus unexpectedly.
- Keep error text near the affected form or operation and make retry controls keyboard accessible.
- Do not expose raw server payloads, stack traces, tokens, or sensitive details.

## Related skills and repository references

- Use dashboard-data-access for query and mutation mechanics.
- Use dashboard-forms-dialogs for submission and validation behavior.
- Use dashboard-authentication for session-expiry behavior.
- Use dashboard-material-ui for accessible progress, error, and dialog components.
- Use dashboard-testing to exercise every meaningful state.

## Verification and definition of done

Test initial pending, success with data, successful empty, initial error, retry, background fetch, mutation pending, mutation success, and mutation failure as applicable. Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular 21.2.13 and TanStack Angular Query 5.101.0. Current code has inconsistent query feedback, so this skill defines approved future behavior rather than copying every existing page. Query Context7 for changed query status names or unfamiliar retry semantics.
