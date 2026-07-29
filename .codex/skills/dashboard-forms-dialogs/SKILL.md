---
name: dashboard-forms-dialogs
description: Build and evolve typed Reactive Forms and Angular Material dialogs in the Dashboard, including nested groups, dynamic FormArrays, conditional validators, reusable form sections, wizard tabs, typed dialog data, request mapping, pending state, and validation feedback. Use for form, validator, dialog, wizard, or form-section changes under Dashboard features.
---

# Dashboard Forms and Dialogs

Preserve the established typed Reactive Forms architecture and keep complex workflows testable.

## Scope and required context: choose the form approach

- Use typed Reactive Forms for current Dashboard work.
- Prefer FormBuilder.nonNullable or explicit non-nullable controls.
- Preserve dedicated form-group types for large nested forms and FormArrays.
- Do not mix Reactive Forms and Signal Forms within one workflow.
- Treat Signal Forms as an explicit pilot or migration requiring current Context7 research, Material compatibility validation, and user approval.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: forms

- Construct the form in the owning dialog or page and pass focused typed groups or arrays to child sections through required signal inputs.
- Use FormArray for repeatable addresses, contacts, bank accounts, allocations, or similar rows.
- Keep domain and request mapping outside the template and usually outside the component.
- Use getRawValue only when disabled fields are intentionally part of the request.
- Preserve user-entered state when submission fails.

## Validate

- Keep custom validators pure and return stable error keys.
- Let optional empty values pass format validators unless the field is separately required.
- Put cross-field or row-completeness validation at the smallest common group.
- When validators depend on another control, update them in one function, call updateValueAndValidity with deliberate event behavior, and clean up subscriptions with takeUntilDestroyed.
- Mark all controls touched after an invalid submit attempt.
- Keep client validation aligned with, but never a replacement for, backend validation.
- Render specific accessible error messages next to the relevant control or group.

## Compose dialogs

- Use typed MAT_DIALOG_DATA and MatDialogRef.
- Reuse the shared dialog shell, action bar, and wizard tabs.
- Keep orchestration and submission in the dialog container; keep sections focused on their form subtree.
- Use explicit add and remove methods for repeatable rows.
- Disable duplicate submission from the mutation pending signal.
- Close with a meaningful result only after success; keep the dialog open after failure.
- Configure responsive width and max-width at the opener.

## Map and test

- Use dashboard-models-mapping for form-to-request conversion.
- Use dashboard-async-ui-state for pending and failure UX.
- Use dashboard-material-ui for control, dialog, and accessibility details.
- Test validators as pure functions, mapping separately, conditional validation, row operations, invalid submit, successful submit, and failed submit.

## Related skills and repository references

Compose with `dashboard-models-mapping`, `dashboard-material-ui`, `dashboard-async-ui-state`, and `dashboard-testing`. Use existing feature forms and dialog components as repository references before introducing a new pattern.

## Verification and definition of done

Run npm run build and relevant npm test checks. Do not add a forms package or migrate form technology without approval.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular Forms 21.2.13 and Angular Material 21.2.13. The broad installed Angular skill's unconditional Signal Forms guidance conflicts with this approved repository decision and must not override it. Query Context7 for Signal Forms or unfamiliar version-sensitive form APIs.
