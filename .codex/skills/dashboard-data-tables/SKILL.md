---
name: dashboard-data-tables
description: Build and extend the Dashboard's reusable server-driven data tables, including typed columns, filters, sorting, paging, counts, cell actions, export requests, shared table state, and feature-service query synchronization. Use for src/app/shared/data-table or feature management pages and services that consume DataTableState.
---

# Dashboard Data Tables

Keep the shared table generic while feature pages and services own business meaning and server contracts.

## Scope and required context: preserve ownership

- Keep generic display, filter editing, sort, page, cell-action, and export state in shared/data-table.
- Keep feature column definitions, labels, filter choices, row actions, page-size policy, and endpoint parameters in the feature.
- Do not add feature-name checks or feature-specific request logic to DataTableComponent.
- Extract shared behavior only when multiple feature pages need the same contract.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: columns and state

- Use readonly DataTableColumn<T> arrays and keys constrained to keyof T.
- Use valueAccessor only for a real derived or nested value.
- Use displayFormatter for display only; do not let it change filter or serialized values.
- Mark non-exportable columns explicitly.
- Give button cells accessible purpose and emit the row plus typed column.
- Keep draft and applied filters distinct when search mode requires explicit application.
- Normalize filter values consistently before comparing state or sending HTTP parameters.

## Coordinate with the server

- Reset page index when an applied filter or page-size change invalidates the current page.
- Send page index, page size, allow-listed sort field, sort direction, and applied filters through the feature service.
- Include normalized table state in the TanStack query key.
- Preserve both filteredCount and totalCount; do not derive server totals from the current page.
- Do not sort or filter only the loaded page when the UI claims server-wide behavior.
- Keep endpoint filter and sort names aligned with the backend table-query allow list.

## Export honestly

- Treat current-page rows, filtered rows, and all rows as distinct scopes.
- Do not label an export as all or filtered when only the current page is loaded.
- Let the feature or a dedicated endpoint obtain additional rows when a broader export is approved.
- Export only explicitly exportable columns and use stable labels.

## Avoid propagation effects

- Prefer a direct computed view of query data.
- If the current imperative service API requires table-state synchronization, keep that effect narrow and one-way.
- Do not copy query result items and counts into extra signals without a user-editable reason.

## Related skills and repository references

Compose with `dashboard-data-access`, `dashboard-models-mapping`, `dashboard-async-ui-state`, and `dashboard-testing`. Use the shared table implementation and matching backend table request as repository evidence.

## Verification and definition of done

- Use dashboard-data-access for query keys and parameters.
- Use dashboard-async-ui-state for pending, empty, and error presentation.
- Use dashboard-material-ui for table, paginator, sort, filter, and accessibility behavior.
- Use dashboard-testing for table utilities and interaction tests.
- Test filter modes, sort, page bounds, count display, cell actions, export scope, normalization, and state equality.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular and Material 21.2.13, TanStack Angular Query 5.101.0, and the repository's TableQueryRequest conventions. Query Context7 only for changed Material table or query adapter APIs.
