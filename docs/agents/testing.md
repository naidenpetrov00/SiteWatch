# Testing Guidance

## Backend Tests
- There is no dedicated .NET test project today.
- If you add backend tests, create a separate `*.Tests` project beside `src/`.
- Keep test names descriptive, such as `CreateCameraCommandHandlerTests`.

## Client Tests
- Use Jest with the Expo preset.
- Prefer `*.test.tsx` or `*.spec.tsx` names.
- Keep tests close to the feature they cover when practical.

## QA Notes
- Do not generate or plan testing unless the user explicitly asks for it.
- Do not create, generate, or modify EF migrations or snapshot files unless the user explicitly asks for that work.
- If a change needs validation, describe the command or scenario instead of running it unless the user explicitly asks.
- Note any coverage gaps or manual verification steps in the pull request summary.
