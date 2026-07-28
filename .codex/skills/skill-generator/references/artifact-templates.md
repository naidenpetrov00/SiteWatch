# Artifact Templates

Use these compact formats to keep the interactive workflow consistent. Adapt labels to the project, but preserve the decision and provenance information.

## Contents

1. Scope brief
2. Architecture finding
3. Existing-skill candidate
4. Coverage map
5. Proposed skill
6. Registry entry
7. Verification record
8. Refresh impact

## Scope brief

```text
REQUESTED SCOPE
<path, layer, subsystem, or bounded area>

MODE
Existing | New | Refresh

IN SCOPE FOR GENERATION
- <capability or directory>

ADJACENT READ-ONLY CONTEXT
- <area>: <why inspection is necessary>

OUT OF SCOPE
- <area>

AUTHORITATIVE VERSION EVIDENCE
- <file>: <technology and resolved version>

OPEN MATERIAL DECISIONS
- <decision or None>
```

## Architecture finding

```text
FINDING
<concise observation and classification: EXPLICIT, CURRENT,
MODERNIZABLE, QUESTIONABLE, or LEGACY>

EVIDENCE
- <file or document reference>

WHY IT MATTERS
<effect on future generated code or skill ownership>

CURRENT TECHNICAL CONTEXT
<Context7-supported framework behavior, when relevant>

RECOMMENDATION
<default and brief tradeoff>

DECISION
<one concrete question whose answer materially changes the result>
```

Do not ask for a decision for minor implementation details. Record the recommended default instead.

## Existing-skill candidate

```text
CAPABILITY
<engineering capability>

FOUND
<skill name, location/source, and version or freshness if known>

GOOD MATCH
- <matching activation, rules, or technology>

MISSING OR CONFLICTING
- <project rule, version, activation, or architecture gap>

OPTIONS
1. Reuse.
2. Reuse with a project wrapper.
3. Adapt or fork.
4. Build a focused local skill.
5. Do not standardize this capability.

RECOMMENDATION
<option and reason>

DECISION
<request approval for the meaningful reuse action>
```

## Coverage map

```text
SCOPE
<requested area>

CAPABILITY COVERAGE

<capability>
-> <DEDICATED SKILL | COVERED BY ANOTHER SKILL |
    EXISTING REUSED SKILL | PROJECT-WIDE RULE |
    CONTEXT7 FALLBACK | INTENTIONALLY NOT STANDARDIZED>
-> <skill/rule/fallback name and one-line rationale>

GAPS
- <uncovered capability or None>

OVERLAPS
- <ownership conflict or None>
```

Every material capability receives one disposition. Do not hide gaps by omitting them.

## Proposed skill

```text
<number>. <skill-name>

PURPOSE
<owned engineering capability>

ACTIVATES
<specific coding tasks or changes>

WHY SEPARATE
<distinct decisions, evolution, and reuse>

TECHNOLOGIES
- <name and exact detected version>

RELATED
- <skill and composition reason>

EXISTING CANDIDATE
<name/source or None suitable>

RECOMMENDATION
Create | Reuse | Reuse + wrapper | Adapt/fork | Omit
<brief reason>
```

End the map with an explicit approval request. Do not write final skill artifacts first.

## Registry entry

Use one level-two heading per skill so `validate_skill_set.py` can verify discovery.

```markdown
## ef-query

**Purpose:** EF Core read and query operations.

**Activates:** Creating or changing EF Core read queries.

**Validated technologies:** .NET 10.x; EF Core 10.0.2.

**Technical validation:** 2026-07.

**Related:** ef-persistence; integration-testing.

**Origin:** Generated locally from project decisions and current framework guidance.

**Location:** `.codex/skills/ef-query/SKILL.md`.

**Status:** Current.
```

For reused skills, record the original source and any project wrapper. Do not paste their instructions into the registry.

## Verification record

```text
SKILL
<name>

STRUCTURAL VALIDATION
PASS | FAIL: <command and result>

SCENARIO 1: NORMAL
Task: <hypothetical task>
Expected skills: <primary and related skills>
Outcome: PASS | REFINE
Evidence: <decisions the skill led to>

SCENARIO 2: ARCHITECTURE-SENSITIVE
...

SCENARIO 3: EDGE OR COMPOSITION
...

CONTEXT7 FALLBACK CHECK
<why fallback did or did not activate>

REFINEMENTS
- <change or None>
```

## Set conflict check

```text
CROSS-SKILL CHECK

Contradictory instructions: None | <details>
Duplicated rule ownership: None | <details>
Overlapping activation: None | <details>
Unclear composition: None | <details>
Architecture conflicts: None | <details>
Context7 fallback conflicts: None | <details>
Freshness gaps: None | <details>

RESULT
PASS | REFINE
```

## Refresh impact

```text
CHANGE DETECTED
<technology/version or architecture change and evidence>

POTENTIALLY AFFECTED
- <skill>: <reason>

CONTEXT7 RESULT
- <focused topic>: <meaningful current behavior>

CLASSIFICATION
- <skill>: Unchanged | Metadata-only revalidation | Guidance update |
  Architecture decision required | Deprecated

PROPOSED CHANGES
- <skill and exact knowledge/metadata delta>

PRESERVED
- <unaffected project or user decision>

DECISION
<approval request before editing>
```

