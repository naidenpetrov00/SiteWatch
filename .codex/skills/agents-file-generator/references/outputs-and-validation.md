# Outputs and validation

Use these compact output shapes. Adapt labels and scenarios to repository evidence; omit irrelevant sections.

## Contents

- [Existing-instruction review](#existing-instruction-review)
- [Conflict report](#conflict-report)
- [Proposed hierarchy](#proposed-hierarchy)
- [Policy summary](#policy-summary)
- [Verification matrix](#verification-matrix)
- [Generated-file audit](#generated-file-audit)
- [Representative scenarios](#representative-scenarios)
- [Completion report](#completion-report)

## Existing-instruction review

```text
INSTRUCTION REVIEW

File and scope:
Rule or topic:
Disposition: KEEP IN AGENTS | MOVE TO SKILL | MOVE TO DOCUMENTATION |
             MERGE / DEDUPLICATE | REWRITE | DELETE AS NOISE | ASK USER
Evidence:
Reason:
Proposed destination or action:
```

Group uncontroversial similar items, but show material reclassifications, conflicts, and user decisions individually.

## Conflict report

```text
CONFLICT

Sources:
Conflicting behavior:
Why it matters:
Repository evidence:
Recommendation:
Decision needed:
```

Do not silently resolve material conflicts.

## Proposed hierarchy

```text
PROPOSED AGENT STRUCTURE

/AGENTS.md
Scope and purpose:
Unique rules:
Why necessary:
Why these rules are not skill content:

<area>/AGENTS.md
Scope and purpose:
Unique local delta:
Why necessary:
Why these rules are not skill content:
```

Invite the user to approve, remove, add, merge, rename, or rescope files. State explicitly that files will not be written until the hierarchy and material policies are agreed.

## Policy summary

Use short labeled statements:

```text
AGREED POLICY SUMMARY

BUILD
<permission and boundary>

EXISTING TEST EXECUTION
<permission and boundary>

TEST CREATION
<separate permission>

MIGRATIONS
<model/configuration, generation, execution boundaries>
```

Continue only with relevant policies. Ask the user to correct or approve the summary before generation.

## Verification matrix

Base verification on affected scope and confirmed commands:

| Affected scope | Change type | Allowed or required checks | Approval boundary | Evidence |
|---|---|---|---|---|
| Repository area | Build/test/lint/typecheck/format as applicable | Exact confirmed commands | Automatic, requested, or approval required | Manifest, script, CI, or documentation path |

Never collapse test-source creation into this matrix; it is a separate policy.

## Generated-file audit

After writing, verify:

- every file was approved and has a meaningful scope;
- root rules are genuinely shared;
- nested files contain local deltas rather than parent duplication;
- no implementation tutorials or lengthy system explanations remain;
- architecture invariants have repository evidence;
- user-policy rules match the approved summary;
- commands and paths still exist;
- generated files, migrations, dependencies, servers, infrastructure, Git, secrets, and destructive actions are unambiguous where relevant;
- skill routing names the real discovery mechanism;
- the root AGENTS file states the approved research sequence: project context, applicable skills, Context7 for unresolved/version-sensitive gaps, then permitted web search;
- no override file exists without an explained override need;
- unrelated user changes remain untouched.

## Representative scenarios

Select scenarios that match the repository. Always cover the policy distinctions they exercise.

### Service or backend bug fix

Determine applicable instruction scopes, skill discovery, allowed targeted build, existing-test execution, and whether supporting-file edits require approval.

### Frontend or UI feature

Determine applicable scopes, confirm that framework implementation details come from skills rather than AGENTS, and select real lint/typecheck/build commands.

### Data-model change

When a migration framework exists, determine whether model/configuration edits are allowed, whether a migration may be created, whether snapshots may change, and whether any database command may run.

### Dependency addition

Determine whether existing packages must be reused first and when production or development dependency approval is required.

### Unfamiliar or version-sensitive API

Confirm the generator first used repository context, then an applicable skill if the local evidence was insufficient or unreliable, then only the missing information from Context7 MCP when available. Permit general web search only when Context7 is unavailable or inadequate, current external information remains necessary, and search is permitted. Confirm this sequence is stated concisely in the generated root AGENTS file.

### Generated output change

Determine the source or generator to modify, whether regeneration is authorized, and whether direct edits are forbidden.

### Git or destructive operation

Confirm preservation of unrelated changes and explicit approval for destructive state changes.

For every scenario report:

```text
SCENARIO
Applicable AGENTS files:
Applicable skill routing:
Generator research evidence:
Allowed verification:
Test-source permission:
Approval required:
Repository-specific documentation policy:
Ambiguity found:
```

Refine the hierarchy when a scenario produces conflicting scopes, missing permission, excessive interruption, or an answer based on assumptions rather than written policy.

## Completion report

Report:

1. created, changed, removed, and intentionally retained instruction files;
2. important rules moved to skills or documentation;
3. confirmed verification commands and their evidence;
4. scenario results and conflict checks;
5. unresolved limitations or policy choices.

Do not declare completion until hierarchy approval, policy approval, generation, scope audit, command verification, and representative scenarios all pass.
