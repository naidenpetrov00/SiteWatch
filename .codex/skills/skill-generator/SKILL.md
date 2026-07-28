---
name: skill-generator
description: Interactively analyze a user-specified area of an existing or new software system and generate, reuse, adapt, validate, register, or refresh a repository-local modular set of coding skills for that scope. Use when the user asks for skills for a particular layer, directory, subsystem, framework area, or technology capability; asks to refresh skills after dependency or architecture changes; or asks which scoped skills need revalidation. Do not use to generate skills for an entire repository unless the user explicitly names that whole repository as the scope.
---

# Skill Generator

Create maintainable skill sets for an explicit software-system area. Treat repository architecture, user decisions, existing skill candidates, and current Context7 knowledge as separate evidence sources. Keep the user involved at decisions that materially change future generated code.

Read [artifact-templates.md](references/artifact-templates.md) before presenting findings, coverage, proposals, verification results, or registry entries. Use `scripts/validate_skill_set.py` after generating or refreshing a set.

## Operating contract

- Require an explicit requested area. If no boundary is given, ask for it before repository analysis.
- Analyze and generate only for that area. Inspect adjacent code only when necessary to understand boundaries, contracts, dependency direction, or representative behavior.
- Do not silently expand generation scope when inspecting adjacent areas.
- Resolve the repository's skill collection before creating files. Store every generated, adapted, forked, or wrapper skill and its registry inside the repository, normally under `<repository>/.codex/skills`. Never default generated artifacts to user/global skill directories. Ask only when repository instructions define multiple plausible repository-local skill collections.
- Ask only questions whose answers materially affect architecture, skill ownership, reuse, or future generated code. Recommend a default with a brief reason when the user is unsure.
- Stop at each explicit approval gate. Do not interpret silence as approval.
- Treat existing code as evidence, not truth.
- Keep skills modular around coherent engineering capabilities. Avoid broad technology catch-alls and API-level fragments.
- Use applicable repository instructions and the `skill-creator` skill when creating or substantially editing each individual skill.
- Do not build, run, migrate, or modify application code unless the user separately authorizes that work. Skill verification normally uses hypothetical tasks and skill validators, not application changes.

## Select the workflow

Classify the requested area before proposing skills:

- **Existing scope**: meaningful code or architecture already exists. Inspect it, identify versions and representative implementations, classify patterns, research current framework behavior, and resolve architecture questions.
- **New scope**: little or no implementation exists. Elicit requirements, identify or select technologies and versions, research current approaches, and establish the area architecture with the user before mapping skills.
- **Refresh**: a registry and skills already exist. Detect version or architecture changes, identify affected skills, research only affected framework behavior, propose deltas, and update only approved knowledge.

If the evidence does not clearly distinguish existing from new, show the evidence and ask the user.

## Phase 1: Establish scope and evidence

1. Restate the requested generation boundary and list any adjacent areas that may need read-only inspection.
2. Read the nearest applicable `AGENTS.md`, architecture documents, ADRs, and folder-local instructions. Do not scan unrelated folders.
3. Inventory only relevant directories, modules, dependency boundaries, and build manifests.
4. For an existing scope, inspect representative implementations for the capabilities present. Include relevant boundaries such as:
   - API or endpoint patterns;
   - application workflows and domain interaction;
   - persistence and transactions;
   - contracts, DTOs, validation, authorization, and error handling;
   - dependency injection and configuration;
   - UI state, forms, routing, and API clients where relevant;
   - tests, build tooling, linting, and formatting.
5. Inspect nearby areas only far enough to explain dependency direction, contracts, shared behavior, or integration seams.
6. For a new scope, collect product constraints, target platforms, integration boundaries, expected workflows, non-functional requirements, and technology preferences.

Record evidence with file references. Separate observations from recommendations.

## Phase 2: Determine exact technologies

Inspect authoritative manifests and lock files before framework-dependent research. Common evidence includes:

- `.csproj`, `Directory.Packages.props`, `global.json`, and SDK pinning files;
- `package.json` and npm, pnpm, Yarn, or Bun lock files;
- `angular.json`, Expo app configuration, native project files, and database configuration;
- tool manifests, build configuration, generated dependency graphs, and CI setup.

Prefer resolved versions from lock files or SDK outputs over loose version ranges. Do not run package installation, restoration, builds, or upgrades unless authorized. Mark a technology version as `unknown` when the available evidence cannot prove it, and ask only if the uncertainty affects the skill map or current guidance.

## Phase 3: Classify architecture and implementation

Classify each material discovered pattern:

- **EXPLICIT**: documented architecture or a user decision.
- **CURRENT**: consistent, intentional behavior worth preserving.
- **MODERNIZABLE**: valid architecture implemented with a replaceable or outdated technique.
- **QUESTIONABLE**: inconsistent or potentially harmful behavior requiring a decision.
- **LEGACY**: obsolete or accidental behavior that must not become new skill guidance.

Do not silently encode QUESTIONABLE or LEGACY patterns. Preserve intentional architecture even when framework examples use a simpler design. Use current implementation techniques inside the chosen architecture.

For each unresolved material issue, present:

1. finding and evidence;
2. why it changes future generated code;
3. current technical context;
4. recommendation and tradeoff;
5. one concrete decision request.

Continue until architecture and convention decisions are sufficient to define capability ownership.

## Phase 4: Research with Context7

Use Context7 as a primary technical source during creation and refresh, not as a ceremonial citation.

1. Build a research list from the proposed capabilities and installed technologies.
2. Resolve the official Context7 library ID for each framework or library. Prefer exact name match, authoritative source, strong documentation coverage, and the installed version. Reuse a resolved ID during the same workflow.
3. Use a versioned Context7 ID when the installed version is available in Context7. Otherwise include the exact installed version in each query and record that the source was not version-pinned.
4. Query one focused implementation concept at a time. Never send secrets, personal data, or proprietary source code.
5. Research the current behavior needed by each capability, including deprecated APIs, changed defaults, superseded workarounds, configuration, performance behavior, and version-sensitive edge cases.
6. Compare the retrieved guidance with the chosen architecture and representative repository code.
7. Convert the result into concise rules, workflows, decision rules, anti-patterns, examples, and verification criteria. Do not copy documentation into skills.
8. Record lightweight provenance as `CURRENT FRAMEWORK GUIDANCE` and record the validation month and exact technology version.

If Context7 is unavailable or lacks the required authoritative source, report the gap. Ask before using web search unless the user already authorized external search. Do not claim framework validation without evidence.

At normal coding runtime, make generated skills the primary guidance. Include a narrow Context7 fallback only for uncovered APIs, uncertain behavior, newer installed versions, unfamiliar capabilities, repository conflicts, or a decision that requires documentation. Retrieve only the missing knowledge.

## Phase 5: Discover existing skills

Search all skill sources available to the agent before proposing creation:

- repository-local skill directories and registries;
- user or global skills;
- installed and bundled skills;
- trusted available catalogs and approved external sources.

Search metadata and registries first; load full candidate instructions only when the description materially overlaps a required capability. Do not disclose or copy inaccessible skill contents.

Classify each candidate:

- **REUSE**: capability and guidance fit without project-specific changes.
- **REUSE + PROJECT WRAPPER**: generic technical guidance is sound but local architecture belongs in a narrow companion skill.
- **ADAPT / FORK**: a maintained copy is justified by substantial project-specific changes or incompatible activation.
- **DO NOT REUSE**: stale, contradictory, overly broad, untrusted, or irrelevant.
- **CREATE NEW**: no suitable candidate exists.

For every meaningful candidate, show its source, good fit, gaps, risks, available options, and a recommendation. Obtain user approval before reuse, wrapping, adaptation, replacement, or forking. Never mutate an installed or third-party skill in place unless the user explicitly requests it and owns that location.

Prefer a focused repository-local skill or narrow project wrapper when a broad generic candidate would load irrelevant context, weaken activation, or conflict with the chosen architecture. Existing skills are candidates, not mandatory dependencies.

A directly reused installed or global skill may remain at its existing source location. Record that external source in the repository registry. Write every new wrapper, fork, adaptation, or replacement into the repository skill collection.

## Phase 6: Build capability coverage

List every relevant engineering capability in the requested area. Assign exactly one disposition:

- `DEDICATED SKILL`
- `COVERED BY ANOTHER SKILL`
- `EXISTING REUSED SKILL`
- `PROJECT-WIDE RULE`
- `CONTEXT7 FALLBACK`
- `INTENTIONALLY NOT STANDARDIZED`

Identify uncovered capabilities and overlaps. Do not create one skill per API call. Give a capability its own skill only when it has distinct activation, meaningful engineering decisions, independent evolution, independent reuse, and enough guidance to justify context cost.

## Approval gate: propose the skill map

Present the coverage map and then a proposed modular skill map. For each proposed or adopted skill include:

- name and purpose;
- activation criteria;
- reason for separate ownership;
- technologies and validated versions;
- related skills;
- existing candidate and source;
- recommendation: create, reuse, wrap, adapt, or omit.

Invite the user to approve, remove, rename, merge, split, add, or change reuse decisions. Do not create, adapt, replace, or adopt final skill files before the map and meaningful reuse decisions are approved.

## Phase 7: Generate the approved set

1. Use the `skill-creator` skill for each new or substantially adapted skill. Follow its initializer, metadata, resource, and basic-validation requirements, but always pass the repository skill collection explicitly as the output path; do not accept its user/global default.
2. Preserve the approved names and capability ownership. Reopen the map if implementation reveals a material overlap or missing capability.
3. Keep each `SKILL.md` concise and imperative. Put detailed or variant-specific material in directly linked references.
4. Include, when relevant:
   - focused description and activation metadata;
   - scope and required repository context;
   - architecture constraints and provenance;
   - workflow and current implementation rules;
   - project conventions and user decisions;
   - decision rules and anti-patterns;
   - related skills and representative repository references;
   - verification, definition of done, and Context7 fallback;
   - validated technologies and technical validation month.
5. Avoid duplicating rules owned by related skills. Describe composition without inventing hard dependencies.
6. Optimize each skill for the normal 90–95% of repeated work in its capability. Leave unusual, new, or genuinely version-sensitive behavior to the narrow Context7 fallback instead of predicting every future API.
7. Encode knowledge priority:
   1. explicit task requirements;
   2. project architecture and repository rules;
   3. applicable skills;
   4. relevant current implementation;
   5. Context7 for uncovered or uncertain technical knowledge;
   6. general model knowledge.

## Maintain the registry

Create or update `INDEX.md` at the repository skill collection root. Keep it lightweight and easy to scan. Record each generated, adapted, wrapped, or adopted skill with:

- name, purpose, and activation summary;
- validated technologies and technical validation month;
- related skills;
- origin and source location;
- status such as `Current`, `Needs revalidation`, or `Deprecated`.

Do not duplicate full skill contents. Preserve registry entries outside the requested scope. Distinguish project rules, user decisions, framework guidance, and reused-skill knowledge sufficiently for later refreshes.

## Phase 8: Verify

1. Run the individual validator required by `skill-creator` for every created or changed skill.
2. Run `python scripts/validate_skill_set.py --root <skill-collection> --registry <INDEX.md>` from this skill directory. Resolve the Python executable available in the environment; do not install it without user approval.
3. Forward-test each important skill against two to five hypothetical coding tasks without modifying the application:
   - a normal repeated case;
   - an architecture-sensitive case;
   - a common edge case;
   - a case where another skill should also activate;
   - optionally, a case that should trigger Context7 fallback.
4. Evaluate whether the skill selects a correct approach, preserves architecture, owns the right decisions, composes cleanly, covers common situations, and avoids unnecessary Context7 use.
5. Refine ambiguous guidance and repeat affected scenarios.
6. Compare the full set for contradictory rules, duplicated ownership, overlapping activation, unclear boundaries, stale validation metadata, and conflicting fallback instructions.
7. Present concise verification evidence and unresolved limitations. Do not call the set complete when a required check is missing or inconclusive.

## Refresh workflow

1. Read the registry and selected skills.
2. Re-detect current framework and library versions from authoritative files.
3. Map changed dependencies or architecture decisions to potentially affected skills.
4. Resolve and query current Context7 documentation only for affected capabilities.
5. Compare framework-derived rules with new guidance while preserving unaffected project and user decisions.
6. Classify each skill as unchanged, metadata-only revalidation, guidance update, architecture decision required, or deprecated.
7. Show proposed changes and obtain approval before modifying skills.
8. Update only affected knowledge, rerun individual and set verification, and update registry metadata.

Do not regenerate unaffected skills. A dependency upgrade creates a revalidation candidate, not proof that every related skill needs edits.

## Completion criteria

Finish only when the requested scope is understood, material decisions are resolved, exact versions are identified or explicitly recorded as unknown, Context7 research is evidenced, existing skills and reuse decisions are resolved, coverage and skill maps are approved, approved artifacts exist in the repository skill collection, scenario and conflict verification pass, and the repository registry reflects the current set.
