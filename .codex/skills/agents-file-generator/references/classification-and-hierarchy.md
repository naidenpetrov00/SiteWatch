# Classification and hierarchy design

Use this guide to decide what belongs in AGENTS files and where it should live.

## Contents

- [Content categories](#content-categories)
- [Existing-rule dispositions](#existing-rule-dispositions)
- [Contradiction audit](#contradiction-audit)
- [Scope design](#scope-design)
- [AGENTS.override.md](#agentsoverridemd)
- [Rule quality test](#rule-quality-test)

## Content categories

### Repository operation — keep in AGENTS

Examples: directory map, navigation, confirmed commands, verification selection, generated directories, inspection boundaries, and local tool constraints.

### Architecture invariant — keep in the narrowest useful AGENTS scope

Examples: forbidden dependency directions, feature isolation, ownership boundaries, and stable separation of persistence, transport, domain, or shared code.

Require evidence that the rule is stable and broadly applicable within the file's scope.

### User policy — keep after agreement

Examples: test creation, migration generation, dependency additions, server startup, external lookup, Git changes, infrastructure access, scope expansion, and approval requirements.

Source code cannot prove user preference. Ask before choosing material behavior.

### Skill-routing rule — keep concise in root AGENTS

Direct agents to discover and use applicable project skills as primary implementation guidance, compose multiple relevant skills, and avoid loading unrelated skills. Name the real registry or discovery path when one exists.

### Task-specific implementation knowledge — move to a skill

Examples: EF query patterns, Angular form construction, endpoint templates, React Query hooks, framework-specific optimization, and detailed generation steps.

Report valuable displaced material as:

```text
SKILL CANDIDATE
Source:
Capability:
Why it does not belong in AGENTS:
Recommended action:
```

Do not create the skill without a separate request.

### Explanatory system knowledge — move to documentation

Examples: business-domain explanations, authentication flows, integration specifications, and deployment tutorials. AGENTS may point to the authoritative document when agents need it.

### Noise — delete

Remove generic exhortations such as "write clean code," "use best practices," "be senior," "think carefully," or "do not make bugs" unless rewritten as a concrete repository-specific constraint.

## Existing-rule dispositions

Assign each meaningful rule one disposition:

- `KEEP IN AGENTS`: correct, actionable, and properly scoped.
- `MOVE TO SKILL`: reusable implementation procedure or framework knowledge.
- `MOVE TO DOCUMENTATION`: explanatory system or operational knowledge.
- `MERGE / DEDUPLICATE`: useful but repeated in a parent, sibling, or related instruction file.
- `REWRITE`: intent is useful but wording is vague, stale, overbroad, or not actionable.
- `DELETE AS NOISE`: no meaningful repository-specific guidance.
- `ASK USER`: a material policy choice or unresolved conflict.

Show the user important reclassifications before changing existing files.

## Contradiction audit

Compare root and nested instructions, override files, command evidence, architecture docs, and skill-routing rules. Flag conflicts that change behavior, including:

- running no tests versus running all tests;
- repository patterns always winning versus skills intentionally modernizing stale techniques;
- automatic migration generation versus explicit approval;
- broad web prohibition versus permitted documentation tooling;
- root permission versus a stricter nested prohibition;
- commands or paths that no longer exist.

Do not silently select a side when the result materially changes architecture, scope, data, security, verification, or approval behavior.

## Scope design

Use this model:

```text
root AGENTS.md
  shared repository operation and policy
  nested AGENTS.md
    stable local deltas and invariants only
```

Create a nested file only if all are true:

1. The subtree is a meaningful work scope.
2. It has stable rules different from or more specific than the parent.
3. Those rules should load for most tasks in that subtree.
4. The rules are substantial enough to justify another instruction boundary.

Folder existence alone is not evidence. Avoid one file per architecture layer when a single root or area file communicates the same rules more clearly.

Place global skill discovery, secrets, Git safety, scope control, repository scanning, general verification, and documentation lookup policy at the root. Place backend-, frontend-, mobile-, domain-, or deployment-specific invariants and commands only in the relevant scope.

Nested files should contain the delta from their parent. Do not repeat global rules for emphasis unless the nested scope must function independently and the user explicitly accepts the duplication.

## AGENTS.override.md

Prefer normal nested `AGENTS.md` files. Propose `AGENTS.override.md` only when the repository needs deliberate replacement or stronger local semantics that ordinary specialization cannot express. Explain its effective scope, what it overrides, and why an ordinary nested file is insufficient.

## Rule quality test

Keep a rule only when it does at least one job:

- prevents a recurring repository-specific mistake;
- communicates a real architecture invariant;
- defines an agreed policy;
- selects real verification;
- routes agents to the right knowledge;
- prevents a dangerous or unwanted action.

Rewrite or remove rules that fail this test.
