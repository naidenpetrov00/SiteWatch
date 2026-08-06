---
name: skill-generator
description: Analyze, create, reuse, refresh, validate, and register a repository-local modular skill set for an explicitly named software-system area. Use when generating skills for a layer, directory, subsystem, framework capability, or the explicitly named whole repository, or when refreshing affected skills after architecture or dependency changes.
---

# Skill Generator

Design the smallest repository-local skill set that preserves distinct activation and engineering ownership. Keep approval gates explicit and load detailed resources only for the phase that needs them.

## Route the workflow

1. Require an explicit scope and classify it as existing, new, or refresh.
2. Read the applicable repository instructions, registry metadata, manifests, and representative source. Inspect adjacent areas only to resolve boundaries.
3. Present a provisional map before external research, full candidate loading, or edits. Include each likely skill, purpose, disposition, evidence inspected, possible files, and open decisions. Wait for explicit approval.
4. After approval, resolve exact versions and research only behavior that remains unresolved after reviewing applicable skills and project context. Use Context7 when available for current, version-specific framework guidance, including technical best practices and modernization decisions; use web search only when permitted and Context7 is unavailable or inadequate.
5. Compare meaningful installed or external candidates only when their metadata materially overlaps the capability. Record why local guidance, a wrapper, adaptation, reuse, or omission is appropriate.
6. Assign every capability to one owner: dedicated skill, another skill, reused skill, repository policy, Context7 fallback, or intentionally unstandardized.
7. Present the final coverage and skill map. Wait for explicit approval before creating, adopting, replacing, or substantially editing artifacts.
8. Use `skill-creator` for each approved new or substantially revised skill. Keep artifacts under the repository's skill collection.
9. Update the registry, validate each changed skill, validate the set, and forward-test important activation and composition cases without changing application code.

## Load phase references

- Before presenting a checkpoint, coverage map, registry entry, or verification result, read [artifact-templates.md](references/artifact-templates.md).
- Run `scripts/validate_skill_set.py` after generation or refresh. Use its duplicate-content option when consolidation is part of the task.
- Do not load candidate bodies merely to enumerate skills; start from catalog or registry metadata.

## Preserve ownership

- Give a capability its own skill only when it has distinct activation, decisions, evolution, reuse, and enough non-obvious guidance to justify runtime cost.
- Treat repository architecture and user decisions as authoritative locally; treat current external documentation as authoritative for versioned APIs and as the primary guide when local technical practices are questionable or legacy.
- Treat existing code as evidence of current behavior, not as proof of best practice. Classify local precedent as current, questionable, modernizable, or legacy before encoding it in a skill.
- Do not encode questionable or legacy precedent without an explicit decision. When local code conflicts with current Context7 or official documentation, surface the mismatch and recommend modernization while preserving compatibility constraints.
- Keep maintenance metadata in the registry and detailed variants in references. Keep normal runtime bodies focused on the repeated 90–95% case.
- A related-skill entry is adjacency, not an instruction to load it. Activate another skill only when the task crosses its owned boundary.
- Preserve registry entries and skill files outside the approved refresh scope.

## Approval and completion

Approval of discovery does not approve artifacts. Approval of a final skill map does not authorize application changes, dependency changes, migrations, servers, or external infrastructure.

Complete the refresh only when approved artifacts exist, registry parity passes, representative activation and non-activation scenarios pass, duplicated ownership is resolved, and unresolved research or verification gaps are reported.
