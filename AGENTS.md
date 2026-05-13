# Repository Guidelines

## Project Overview
SiteWatch is split into a .NET Clean Architecture backend under `src/` and an Expo React Native client under `src/ClientApp/`.

## Global Rules
- Do not build, run, or launch projects unless the user explicitly asks.
- Do not commit secrets or environment-specific values.
- Keep changes scoped and avoid unrelated formatting or churn.
- Follow the repository’s existing naming and formatting conventions.

## Specialized Guidance
- Read `docs/agents/core.md` for shared repository conventions and layout.
- Read `docs/agents/backend.md` for `Api` / `Application` / `Domain` / `Infrastructure` work.
- Read `docs/agents/frontend.md` for Expo Router and React Native work.
- Read `docs/agents/testing.md` for tests and QA.
- Read `docs/agents/deployment.md` for environment, configuration, and release concerns.

## Usage
Load only the specialized file relevant to the current task.
