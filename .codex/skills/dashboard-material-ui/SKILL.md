---
name: dashboard-material-ui
description: Build and review accessible Angular Material and CDK interfaces for the Dashboard, including component imports, Material 3 theming, shared UI composition, dialogs, menus, responsive component CSS, focus behavior, and native animations. Use for Angular Material, CDK, material-theme.scss, shared UI, or accessibility-sensitive template and style changes.
---

# Dashboard Material UI

Use the installed Material and CDK packages as the primary UI system and preserve the existing Material 3 token architecture.

## Scope and required context: select and compose UI

- Reuse existing shared action buttons, dialog shell, dialog action bar, wizard tabs, date picker, and data table when their contracts fit.
- Import only the standalone Material or CDK modules used by a component.
- Prefer Material components for established controls and CDK primitives for behavior that Material does not own.
- Keep feature-specific labels, validation, and workflows in the feature.
- Do not add @angular/aria or another component package without explicit dependency approval and a demonstrated gap.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: theming

- Maintain the global Sass theme in src/material-theme.scss with the mat.theme mixin.
- Use Material 3 system variables such as --mat-sys-surface and --mat-sys-primary in application CSS.
- Keep color, typography, and density changes centralized when they affect the whole application.
- Avoid styling Material internals by generated class names or ::ng-deep. Prefer supported tokens, public APIs, wrapper layout styles, and documented mixins.
- Keep density usable for pointer and assistive-technology users.
- Use responsive component CSS and test narrow viewports for dialogs, navigation, forms, and tables.
- Prefer native CSS transitions and animate.enter or animate.leave. Do not introduce the legacy Angular animation DSL for new work.

## Preserve accessibility

- Start with native semantic elements and correct Material controls.
- Give navigation landmarks and dialogs useful labels and headings.
- Give icon-only buttons an aria-label.
- Preserve keyboard operation, visible focus, logical focus order, and adequate target sizes.
- Expose expanded, selected, current, invalid, and busy states when the chosen component does not do so automatically.
- Do not assume Material removes the need to test accessible names, focus restoration, validation announcements, or color contrast.

## Dialog rules

- Open dialogs from the owning feature page or orchestrator.
- Pass typed MAT_DIALOG_DATA and use a typed MatDialogRef.
- Reuse the shared dialog shell and action bar for consistent structure.
- Keep dialogs within the viewport with responsive width and max-width settings.
- Preserve form state on a failed submission and close only after confirmed success or explicit cancellation.

## Related skills and repository references

Compose with `dashboard-components-templates`, `dashboard-forms-dialogs`, `dashboard-async-ui-state`, and `dashboard-testing`. Inspect `src/Dashboard/src/styles.scss` and representative Material components as repository evidence.

## Verification and definition of done

- Prefer Material component harnesses for behavior whose DOM is an implementation detail.
- Test keyboard-visible behavior and accessible labels for custom composition.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular Material and CDK 21.2.13. Current official guidance confirms the Material 3 mat.theme and system-variable approach. Query Context7 or official Material documentation narrowly for an uncovered component API, token, harness, or version change.
