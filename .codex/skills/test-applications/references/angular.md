# Angular testing

## Default stack

Use the Dashboard's Angular CLI unit-test builder, Vitest, and jsdom. Prefer first-party Angular APIs and CDK/Material harnesses before proposing another component dependency. Use Playwright for new browser end-to-end coverage only after approval.

## Pick the surface

- Test pure functions, mappers, validators, and reducers directly without `TestBed`.
- Use `TestBed` when DI, templates, change detection, routing, HTTP, or providers are behavior.
- Use component harnesses for Material and reusable complex controls.
- Use `RouterTestingHarness` with real test routes for guards, resolvers, redirects, and navigation; do not replace the router with a loose spy.
- Use Angular's HTTP testing provider and controller for request behavior; never call the real network.

## Assert as a user

Query by accessible role, label, name, or visible text where practical. Prefer harness APIs over DOM structure for Material controls. Use stable application selectors only when no semantic query fits.

Interact through the DOM or harness. Assert visible output, enabled state, focus, validation, navigation, and emitted outcomes rather than private fields or internal calls.

Account for zoneless async rendering: act, await stability or the relevant result, then assert. Use manual `detectChanges()` only when deliberately required, not as a universal fix.

## Vitest discipline

- Restore spies and global state in lifecycle hooks.
- Use fake timers only for real timer behavior; advance explicitly and restore real timers.
- Prefer tables for validation and mapping rules.
- Avoid broad markup snapshots.
- Treat jsdom as an emulator, not proof of browser-specific behavior.

Provide typed fakes or Vitest spies for narrow service boundaries. Avoid mocking every child or Angular primitive. Keep real signals, forms, router configuration, and HTTP test infrastructure when they are under test.

Reserve Playwright for a compact set of critical journeys. Select by accessible role and name, seed controlled state, and avoid shared journey state.
