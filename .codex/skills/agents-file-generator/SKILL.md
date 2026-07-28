---
name: agents-file-generator
description: Analyze a repository and interactively create, review, clean up, restructure, or refresh its AGENTS.md instruction hierarchy. Use when the user asks to create or revise repository agent instructions, audit existing AGENTS.md or AGENTS.override.md files, add scoped instructions for a new application or module, change repository operating policies, or route coding agents through project skills without embedding framework tutorials in AGENTS files.
---

# Agents File Generator

Create the smallest persistent instruction hierarchy that reliably guides coding agents through the repository's architecture, tools, skills, verification, and approval boundaries.

Read these references before acting:

- Read [discovery.md](references/discovery.md) before repository inspection.
- Read [classification-and-hierarchy.md](references/classification-and-hierarchy.md) before reviewing existing instructions or proposing scopes.
- Read [policy-interview.md](references/policy-interview.md) before asking policy questions or revising user-policy rules.
- Read [outputs-and-validation.md](references/outputs-and-validation.md) before presenting a proposal, policy summary, final files, or validation results.

## Operating contract

- Treat `AGENTS.md` as repository operating policy, not an implementation handbook.
- Put stable architecture invariants, navigation, skill routing, verified commands, scope rules, and important safety or approval rules in AGENTS files.
- Keep detailed engineering procedures in modular skills and explanatory system knowledge in repository documentation.
- Inspect broadly enough to understand repository operation and boundaries, but do not scan every source file.
- Infer facts that repository evidence proves. Discuss policy choices that source code cannot safely determine.
- Ask questions in small, relevant groups. Explain the evidence and tradeoff, recommend a default, and request one concrete decision.
- Stop before writing final AGENTS files until the user has approved the proposed hierarchy and the material policies. Do not interpret silence as approval.
- Preserve unrelated working-tree changes. Do not use destructive Git or filesystem operations.
- Confirm every command from repository evidence. Never invent build, test, lint, formatting, migration, or run commands.
- Do not create `AGENTS.override.md` routinely. Propose it only when deliberate stronger override semantics are necessary and explain why.
- Report detailed implementation guidance found in AGENTS files as a `SKILL CANDIDATE`; do not create the skill unless requested.

Keep this skill separate from any repository `skill-generator`: this skill defines how coding agents operate in the repository, while a skill generator defines how they perform specific engineering capabilities. If an AGENTS audit discovers useful implementation knowledge, report the candidate and defer creation or migration to a separate skill-generation request.

## Select the workflow

Choose one workflow from current evidence:

- **Existing hierarchy:** one or more useful `AGENTS.md`, `AGENTS.override.md`, or related agent-instruction files exist. Audit their scope, content, duplication, age, breadth, contradictions, and placement before proposing changes.
- **New hierarchy:** no useful agent instructions exist. Discover repository structure, commands, architecture, skills, and relevant policies before proposing files.
- **Targeted refresh:** the user identifies a localized change such as a new frontend, revised testing policy, or added skill registry. Inspect affected scopes and parent instructions, then update only what the change requires.

Treat deleted or renamed instruction files in a dirty worktree as user state, not as permission to restore them.

## Phase 1: Discover the repository

1. Resolve the repository root and applicable existing instructions.
2. Inventory applications, projects, package managers, dependency manifests, build systems, test projects, lint and formatting tools, CI, hooks, deployment configuration, architecture documents, ADRs, READMEs, skills, registries, generated-code areas, migrations, and common commands.
3. Inspect manifests, configuration, scripts, documentation, and representative boundary files. Expand into source only when necessary to prove module boundaries or dependency direction.
4. Record findings with file evidence and distinguish proven facts from tentative inferences.
5. Identify existing uncommitted changes before proposing edits.

Follow [discovery.md](references/discovery.md); adapt its examples to the repository rather than assuming a technology stack.

## Phase 2: Audit and classify

For an existing hierarchy:

1. Map each instruction file to its effective scope and parent.
2. Classify every meaningful rule as `KEEP IN AGENTS`, `MOVE TO SKILL`, `MOVE TO DOCUMENTATION`, `MERGE / DEDUPLICATE`, `REWRITE`, `DELETE AS NOISE`, or `ASK USER`.
3. Compare root, nested, override, documentation, skill-routing, and command evidence for contradictions.
4. Present material reclassifications and conflicts before rewriting.

For a new hierarchy, classify discovered facts into repository operation, architecture invariant, candidate user policy, skill-routing rule, implementation knowledge, explanatory documentation, or noise.

Use [classification-and-hierarchy.md](references/classification-and-hierarchy.md) for placement decisions.

## Phase 3: Propose the hierarchy

1. Start with a root `AGENTS.md` for shared rules.
2. Add a nested `AGENTS.md` only when a subtree has important, stable rules that specialize the parent and are likely to matter whenever work occurs there.
3. Keep nested files as deltas. Do not repeat global security, Git, skill, or scope rules without a demonstrated independent need.
4. Prefer architecture invariants over file-placement tutorials.
5. Present the proposed file tree before writing. For each file, explain its scope, unique rules, necessity, and why those rules do not belong in a skill.
6. Invite the user to approve, remove, add, merge, rename, or rescope files.

Do not create final instruction files until this structure is sufficiently agreed.

## Phase 4: Resolve user policies

Build the interview from repository findings. Ask only policies that matter to the current repository, in small logical groups.

Always distinguish:

- running existing verification from creating or modifying test source;
- build, test, lint, and typecheck from launching applications, servers, containers, or infrastructure;
- model or configuration edits from migration generation and database execution;
- supporting files needed for the same requested behavior from materially expanded product scope;
- approved documentation tools such as Context7 from unrestricted general web search.

Apply safe inferred rules for secrets, unrelated user changes, destructive actions, and production access unless the user explicitly chooses a stricter compatible policy. Use [policy-interview.md](references/policy-interview.md) for relevant choices and recommended defaults.

After decisions are resolved, present one concise policy summary and ask the user to correct or approve it. Stop before generation until material policies are agreed.

## Phase 5: Generate or update files

1. Write only the approved files.
2. Keep rules concise, actionable, repository-specific, and scoped.
3. Include only commands confirmed by repository evidence. Define verification by affected scope instead of requiring one giant command for every change.
4. Route implementation work through the repository's actual skill discovery mechanism. Do not duplicate skill contents in AGENTS files.
5. Define Context7 or other documentation lookup as a narrow fallback for missing, uncertain, unfamiliar, or version-sensitive technical knowledge when the approved policy permits it.
6. Do not copy secrets, credentials, environment values, or private configuration into instructions.
7. Do not direct agents to edit generated outputs unless the repository intentionally treats those files as source.
8. Preserve useful existing rules after reclassification; do not blindly preserve stale or noisy wording.
9. On refresh, modify only affected scopes unless the change exposes a real hierarchy-wide conflict.

## Phase 6: Validate the hierarchy

1. Re-read every generated file with its parent scope.
2. Verify each command against manifests, scripts, CI, or documentation.
3. Check for duplicated rules, contradictory scopes, stale paths, excessive interruptions, misplaced implementation knowledge, and unjustified nested files.
4. Simulate representative tasks covering backend or service work, UI work, data-model changes and migrations when present, dependency additions, and unfamiliar external APIs.
5. For each scenario, identify applicable AGENTS files, skill routing, allowed verification, required approvals, and any documentation fallback.
6. Refine ambiguous rules and repeat affected scenarios.
7. Report created, updated, removed, or intentionally retained files; important reclassifications; validation evidence; and unresolved limitations.

Use [outputs-and-validation.md](references/outputs-and-validation.md) for the required proposal and validation shapes.

## Completion criteria

Finish only when repository structure and applicable commands are evidenced, existing instructions are reviewed when present, material policies and hierarchy are approved, AGENTS content is separated from skills and documentation, approved files are generated, parent and nested scopes are consistent, contradictions and unnecessary duplication are resolved, and representative scenarios show unambiguous behavior.
