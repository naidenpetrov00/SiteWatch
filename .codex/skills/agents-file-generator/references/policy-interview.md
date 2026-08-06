# Policy interview

Use repository evidence to select relevant questions. Do not ask the entire catalog. Ask in small logical groups, and for each decision provide: discovered evidence, the operational choice, a recommended default with a short reason, and one concrete question.

## Contents

- [Build and verification](#build-and-verification)
- [Existing test execution](#existing-test-execution)
- [Test creation and modification](#test-creation-and-modification)
- [Data models, migrations, and database commands](#data-models-migrations-and-database-commands)
- [Dependencies](#dependencies)
- [Applications, servers, containers, and infrastructure](#applications-servers-containers-and-infrastructure)
- [Formatting and lint fixes](#formatting-and-lint-fixes)
- [Repository inspection](#repository-inspection)
- [Scope expansion](#scope-expansion)
- [Existing-code reuse](#existing-code-reuse)
- [Skills and registry](#skills-and-registry)
- [Generator research sources](#generator-research-sources)
- [Secrets and sensitive values](#secrets-and-sensitive-values)
- [Generated files](#generated-files)
- [Git](#git)
- [Destructive actions](#destructive-actions)
- [Documentation updates](#documentation-updates)
- [Ambiguity and communication](#ambiguity-and-communication)
- [Policy-summary requirement](#policy-summary-requirement)

## Build and verification

Distinguish targeted build, test, lint, typecheck, and formatting commands from application startup.

Recommended default: allow relevant targeted verification automatically after changes; avoid expensive repository-wide checks for trivial edits; do not launch applications or long-running processes unless requested.

Include only commands confirmed from repository evidence.

## Existing test execution

Ask whether agents may run existing targeted tests after code changes and when broader suites are expected.

Recommended default: allow targeted existing tests automatically; run broader suites when the affected boundary or repository policy justifies their cost.

This permission never implies permission to create or modify test source.

## Test creation and modification

Resolve separately from test execution. Useful choices include:

- never unless explicitly requested;
- when an applicable skill requires it;
- for behavioral changes by default;
- ask before adding coverage.

Recommend the option that matches repository test maturity and the user's desired maintenance model. Do not hide this choice inside a vague "testing" rule.

## Data models, migrations, and database commands

When a migration system exists, separate:

1. modifying models and configuration as part of the requested behavior;
2. generating, deleting, or editing migrations and snapshots;
3. applying migrations or executing database commands.

Recommended default: allow requested model/configuration changes; require an explicit request for migrations and snapshots; require approval before executing commands that mutate a database.

Clarify local, development, staging, and production access separately when those environments exist. Never infer production permission.

## Dependencies

Ask whether agents may add new production and development dependencies.

Recommended default: reuse existing dependencies first and ask before adding a new production dependency unless the task explicitly requires it. Consider allowing small development-only tooling changes only when repository practice supports them.

## Applications, servers, containers, and infrastructure

Separate short-lived verification from launching development servers, applications, Docker stacks, emulators, or external services.

Recommended default: do not launch long-running processes or connect to external infrastructure unless requested. Treat staging and production as explicit-approval scopes.

## Formatting and lint fixes

Discover configured tools and whether they support targeted operation.

Recommended default: allow targeted formatting and non-destructive lint verification; avoid repository-wide formatting or broad autofix when it could create unrelated churn.

## Repository inspection

Recommended default: start from task-relevant files, then expand inspection when necessary for architecture, reuse, or correctness. Do not perform broad unrelated scans.

Ask only if the user wants a meaningfully different inspection boundary.

## Scope expansion

Recommended default: allow inspection and modification of supporting files required to implement the same requested behavior correctly; ask before adding new functionality, broad refactors, or unrelated improvements.

Do not require approval for every normal supporting edit.

## Existing-code reuse

Recommended default: search the relevant scope for reusable implementations before duplicating them; introduce shared abstractions only for a concrete reuse or ownership need.

Keep framework-specific reuse procedures in skills.

## Skills and registry

When project skills exist, recommend:

1. record skills explicitly named by the user in the working plan and use them as required inputs;
2. do not assume the named skills provide complete task coverage;
3. independently identify uncovered capabilities and add only the additional applicable skills needed;
4. use the selected skills as primary implementation guidance and compose them when the task crosses owned boundaries;
5. avoid loading unrelated skills;
6. use the registry or index before scanning additional skill bodies when one exists.

Confirm the actual discovery path and registry name.

## Generator research sources

Before forming a recommendation or asking a policy question that needs technical evidence, the generator must use:

```text
repository context
  -> user-named skills as required inputs, without assuming complete coverage
  -> only the additional applicable skills needed for uncovered capabilities
  -> Context7 MCP for unresolved or version-sensitive gaps and questionable, stale, or legacy technical patterns
  -> general web search only as the final permitted fallback
```

Use Context7 only when framework or library behavior remains uncovered, uncertain, unfamiliar, version-sensitive, newer than skill validation, or represented locally by a questionable, stale, or legacy technique; retrieve only the missing or questionable technical information. Treat local code as evidence of current behavior rather than proof of best practice. Surface mismatches with current guidance and recommend a compatible resolution instead of silently overriding user policy or repository architecture. Do not use general web search unless Context7 is unavailable or insufficient and external/current information is still needed.

Do not claim Context7 exists unless the environment or repository proves it.

Include this as a concise root AGENTS rule by default. It is a research-source priority, not an instruction to search routinely: agents stop once an earlier source answers the task reliably. Ask only if the user wants a different repository-specific policy.

## Secrets and sensitive values

Normally infer this conservative rule without an interview: never commit, expose, echo, or copy secrets, credentials, tokens, certificates, private keys, or sensitive environment values. Do not place secret contents in AGENTS files.

Ask only if the repository needs stricter handling or specialized secret tooling.

## Generated files

Identify generated outputs and whether the repository intentionally versions any of them.

Recommended default: do not manually edit generated output unless the repository explicitly treats it as source; change its source or generator and regenerate only when authorized.

Migration snapshots remain governed by the migration policy, not merely this general rule.

## Git

Recommended baseline:

- preserve unrelated user changes;
- do not create commits or branches unless requested;
- do not amend, reset, stash, force checkout, or discard changes without explicit approval;
- avoid destructive Git operations.

Ask whether automatic commits or branches are desired only when relevant to the user's workflow.

## Destructive actions

Normally require explicit approval for deleting substantial data or directories, resetting Git state, dropping databases, destructive migrations, overwriting environment configuration, or destructive production operations.

Do not make ordinary scoped file editing approval-heavy.

## Documentation updates

Recommended default: update documentation when the task changes behavior, commands, or architecture that existing documentation explicitly describes. Do not require documentation churn after every edit.

## Ambiguity and communication

Recommended default: resolve minor implementation ambiguity from repository context, applicable skills, and established conventions. Ask when reasonable interpretations would materially change behavior, architecture, scope, data, security, cost, or another important outcome.

Keep communication rules minimal: be concise, report blockers clearly, and ask before materially expanding scope.

## Policy-summary requirement

Before generation, summarize only relevant agreed policies under explicit labels such as:

```text
BUILD
EXISTING TEST EXECUTION
TEST CREATION
MIGRATIONS
DEPENDENCIES
SERVERS
GIT
REPOSITORY INSPECTION
SCOPE
SKILLS
<only repository policy approved for generated AGENTS; do not copy the generator's internal research order>
```

Ask the user to correct or approve the summary. Do not treat earlier silence as agreement.
