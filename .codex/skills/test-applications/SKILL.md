---
name: test-applications
description: Design, create, review, and modernize unit, component, integration, API, and end-to-end tests across the SiteWatch .NET, Angular, and Expo/React Native applications. Use for test strategy, test-source changes, regression coverage, mocking and fixture decisions, testability reviews, or selecting testing libraries for code under src/Domain, src/Application, src/Infrastructure, src/Api, src/Dashboard, or src/ClientApp.
---

# Test Applications

Create durable tests around observable behavior. Treat existing tests as evidence, not authority: preserve sound conventions, replace questionable practices in new work, and do not spread legacy patterns.

## Start with the change set

1. Read the root `AGENTS.md` and the scoped instructions for both the test path and the production behavior under test.
2. Inspect the changes relative to `main` with `git diff main...HEAD`. Also inspect staged and unstaged changes when present, since the requested change may not be committed yet. If `main` is unavailable, report that the comparison could not be made rather than silently choosing an unrelated baseline.
3. Identify the changed behavior, its observable contract, and the risks introduced by each meaningful change. Do not infer test work from changed lines alone; understand the behavior those lines implement.
4. Inspect the relevant manifest, runner configuration, production code, and a small representative set of nearby tests. Search for tests by subject, method, endpoint, component, and behavior—not only by filename.
5. Classify local precedent as current, modernizable, questionable, or legacy. Follow repository architecture and permissions, but prefer this skill over weak local testing style.
6. Read only the needed reference:
   - .NET and ASP.NET Core: [dotnet.md](references/dotnet.md)
   - Angular Dashboard: [angular.md](references/angular.md)
   - Expo/React Native ClientApp: [expo-react-native.md](references/expo-react-native.md)
   - Test-level, data, fixture, and coverage decisions: [test-design.md](references/test-design.md)

Use `references/test-design.md` as the testing handbook and apply its risk, case-set, test-double, and quality rules together with the focused framework reference. Keep the decision process short and concrete: changed behavior -> credible risk -> smallest test level -> existing test to update or new test to add.

## Implement when implementation is requested

- Confirm the user authorized test-source changes. Treat authorization to run tests or add dependencies as separate decisions.
- When the request is to write or implement tests and authorization is present, edit the appropriate test files after inspection. A brief plan is useful, but it must not replace the implementation.
- When the request is only for review, strategy, or explanation, do not modify test files.
- Do not add, replace, or upgrade dependencies without approval. Do not run project commands unless execution is explicitly authorized.

## Choose the smallest valuable test

- Use a unit test for pure rules, transformations, validation, and isolated orchestration.
- Use a component test for rendered behavior and user interaction.
- Use an integration test for framework wiring, serialization, persistence, routing, authentication, or external adapters.
- Use an end-to-end test only for critical journeys whose cross-system behavior cannot be proven more cheaply.

Prefer a focused regression test at the defect boundary. Do not repeat the same assertion at every layer unless each protects a distinct risk.

## Reuse and update existing coverage

- Before adding a test, locate the existing tests for the changed method, class, endpoint, component, or behavior.
- If an existing test already covers the changed behavior, update that test to reflect the new contract. Do not add a second test merely because the production method changed.
- Add a new test only when the change introduces a distinct behavior, boundary, failure mode, or regression risk that existing tests do not cover.
- Do not duplicate equivalent tests within the same test level or repeat coverage across layers unless each test protects a different integration boundary.
- Preserve valuable existing scenarios while removing or reshaping assertions that describe the old contract. Keep the final suite focused and explainable.

## Design tests around behavior

- Name tests as behavior and outcome. Make a failure explain the broken contract.
- Keep Arrange, Act, and Assert distinct. Exercise one behavior while allowing related outcome assertions.
- Assert public results, persisted state, emitted messages, navigation, or visible UI. Verify collaborator calls only when the interaction is the contract.
- Cover the representative success case, boundaries, and credible failures. Do not chase line coverage with implementation-detail tests.
- Control clocks, identifiers, randomness, culture, storage, network, and database state at explicit boundaries.
- Avoid sleeps, timing guesses, order dependence, shared mutable fixtures, real internet calls, and ambient developer-machine state.
- Use theories or tables when inputs vary under one rule. Use focused builders when repeated setup obscures intent.
- Keep literal values that explain the example. Extract duplicated or incidental values; do not hide every value behind generic generation.

## Use doubles deliberately

- Prefer real values and pure collaborators. Substitute only boundaries that are slow, nondeterministic, destructive, unavailable, or needed to force an impractical failure.
- Use an established mocking library. For new .NET tests, prefer NSubstitute over handwritten hardcoded mocks or a custom mocking framework.
- Prefer a small explicit fake when useful state is clearer than dense mock setup. Do not build a reusable fake ecosystem without demonstrated reuse.
- Avoid deep stubs, mock-everything arrangements, broad `any` matchers, and incidental call-order assertions.
- Do not change production architecture solely for a mocking tool. Introduce a boundary only when it expresses real ownership or nondeterminism.

## Select dependencies responsibly

Reuse a current, appropriate installed runner or library. If the project uses a weak or deprecated approach, do not copy it into new tests: recommend the preferred replacement and smallest migration boundary.

Ask before adding, replacing, or upgrading dependencies. Resolve compatible versions from the manifest and current official documentation; do not copy versions from this skill blindly.

## Complete the task

Keep changes within the authorized scope and preserve unrelated work. Do not edit generated output or migrations. Run only the exact test, build, lint, or coverage command explicitly authorized and derived from repository configuration. Otherwise perform static verification and state that tests were not run.

Report behavior covered, design decisions, files changed, execution performed, and remaining risks or approvals.
