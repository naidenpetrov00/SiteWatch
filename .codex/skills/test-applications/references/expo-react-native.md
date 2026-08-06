# Expo and React Native testing

## Default stack

Keep Jest with `jest-expo`. Use React Native Testing Library for new component and screen tests; do not add tests that drive `react-test-renderer` directly. Install compatible packages through the chosen package manager and Expo compatibility guidance only after approval.

Use Maestro for native end-to-end journeys only when device coverage and setup are explicitly requested.

## Pick the surface

- Test pure TypeScript rules, mappers, selectors, and reducers directly.
- Use React Native Testing Library for components, providers, navigation behavior, and user interaction.
- Use a small render helper for genuinely shared providers. Create fresh mutable QueryClient, store, and router state per test.
- Test API clients separately. At component level, replace the Application-owned network boundary or use approved request interception.

## Assert as a user

Prefer role, accessible name, label, placeholder, and visible text. Use `testID` only when semantic queries cannot identify an element. Prefer `userEvent` for supported interactions and await it.

Assert rendered content, accessibility state, navigation, cached state, or visible errors. Avoid component-instance access and callback-wiring assertions.

## Jest and native boundaries

- Use `jest-expo` for standard native-module behavior.
- Mock only the narrow native or application boundary needed. Keep mock factories typed and local unless reuse is demonstrated.
- Reset mocks and timers. Create fresh mutable provider state.
- Avoid real storage, media, camera, network, filesystem, and device APIs in unit/component tests.
- Prefer a controllable adapter fake over a large set of unrelated Jest mocks.

Await user interactions and use `findBy*` or `waitFor` for observable async results. Never sleep or arbitrarily flush promises. Treat unwrapped update warnings as defects.

Avoid broad tree snapshots. For routing, exercise a supported route environment; mock navigation only when requesting navigation is the complete contract.

Use Maestro for a few journeys requiring an installed app, permissions, deep links, or platform integration. Keep device state and accounts isolated.
