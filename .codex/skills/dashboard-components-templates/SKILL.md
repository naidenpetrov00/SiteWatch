---
name: dashboard-components-templates
description: Build and refactor standalone Angular Dashboard components, directives, signal inputs and outputs, computed state, effects, lifecycle cleanup, content projection, and modern templates. Use for .ts, .html, and component style changes under src/Dashboard/src/app that are not primarily routing, forms, tables, or data-access work.
---

# Dashboard Components and Templates

Build strict, signal-based UI while keeping component state understandable.

## Scope and required context: inspect before editing

1. Read the applicable AGENTS.md files and the owning feature.
2. Reuse nearby standalone component conventions and shared UI only when ownership matches.
3. Inspect the template and component styles together with the TypeScript class.
4. Compose with dashboard-material-ui, dashboard-forms-dialogs, or dashboard-data-tables when those concerns dominate.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: components

- Keep standalone metadata implicit and list every template dependency in imports.
- Use ChangeDetectionStrategy.OnPush for nontrivial components, matching current Dashboard practice.
- Use inject() in field initializers for Angular-provided dependencies.
- Declare signal inputs with input() or input.required() and events with output().
- Keep externally exposed state readonly. Use computed() for derived state.
- Use model() only when true two-way component ownership is intentional; prefer input plus output for explicit state flow.
- Use host metadata for host attributes, classes, styles, and events instead of HostBinding or HostListener in new code.
- Use takeUntilDestroyed for manual observable subscriptions tied to the component lifetime.

## Manage state without propagation effects

- Use writable signals for local user-controlled state.
- Use computed() for strictly derived values.
- Use linkedSignal only when a derived default must remain user-overridable.
- Reserve effect() for imperative non-signal boundaries such as storage, a third-party widget, or an existing imperative service API.
- Do not set one signal from another signal inside an effect merely to keep them synchronized.
- Read TanStack Query result signals directly or derive view state with computed rather than copying query data into duplicate writable signals.

## Write templates

- Use @if, @for, and @switch instead of introducing legacy structural syntax.
- Supply a stable track expression for every @for. Prefer a domain identifier; use the object only when identity is intentionally stable.
- Preserve semantic HTML before adding ARIA. Give icon-only controls accessible names and expose expanded, selected, busy, and invalid states when needed.
- Keep templates declarative. Move nontrivial normalization, calculations, and request construction to typed functions or mappers.
- Use content projection for reusable shells with stable slots, as in the shared dialog components.
- Use $any only at a narrow browser-event boundary when strict template typing cannot express the target; do not use it to silence model errors.

## Style components

- Keep component-specific CSS beside the component and use :host for host layout.
- Prefer responsive grid or flex layouts and existing Material system tokens.
- Avoid ::ng-deep and ViewEncapsulation.None unless an explicit library integration proves they are necessary.
- Prefer native CSS transitions and Angular animate.enter or animate.leave for new animation work.

## Related skills and repository references

Compose with `dashboard-feature-architecture`, `dashboard-material-ui`, `dashboard-async-ui-state`, and `dashboard-testing` as the task requires. Treat `src/Dashboard/src/app` and its nearest `AGENTS.md` as repository evidence.

## Verification and definition of done

- Test inputs, outputs, user interaction, derived state, cleanup-sensitive behavior, and accessible names.
- Use dashboard-testing for TestBed, harness, and async-rendering conventions.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular 21.2.13 and TypeScript 5.9.3 against current Dashboard code and official Angular component and signal guidance. Use Context7 for uncovered Angular APIs, experimental signal behavior, or a newer installed version.
