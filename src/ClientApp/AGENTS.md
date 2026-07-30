# Client application instructions

## Scope and ownership

- This file applies to `src/ClientApp`.
- ClientApp is an Expo/React Native application using Expo Router and a development build. File-based routes live under `app`; reusable feature behavior belongs under the existing `features`, `components`, `lib`, `store`, `hooks`, and `types` ownership boundaries.
- Preserve the established API-client and TanStack Query boundaries. Search the affected feature for an existing query, mutation, cache key, component, or model before introducing a parallel implementation.

## Skill routing

- Use `vercel-react-native-skills` for React Native performance and component guidance.
- Use the applicable Expo skill for Expo Router, native UI, development-client, module, deployment, or platform-specific work.
- Use `expo:native-data-fetching` for network requests, API calls, or data-fetching work.
- Keep concrete React Native and Expo syntax or procedures in the selected skills; use Context7 only for unresolved or version-sensitive gaps.

## Development-build and command boundaries

- The app uses a development build. If the user authorizes a native action, distinguish starting the existing development build, rebuilding the client, running a named platform, and generating native projects; authorization for one does not imply the others.
- `package.json` declares Yarn while the repository also contains `package-lock.json` and npm-oriented README guidance. Do not silently choose a package manager. If the user explicitly requests a dependency or command operation without naming one, clarify which package manager to use.
- Do not directly edit `.expo`, `node_modules`, generated native/build output, or captured device artifacts. Treat `.env` as sensitive and never reveal its contents.
