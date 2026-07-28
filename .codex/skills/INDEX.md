# Skill Index

## prompt-generator

**Purpose:** Turn repository work requests into concise, execution-ready prompts with relevant skill and model guidance.

**Activates:** The user invokes `/prompt` or asks for a prompt for another implementation agent.

**Validated technologies:** Codex skill format; repository-local model catalog.

**Technical validation:** Not recorded by the existing skill.

**Related:** skill-generator.

**Origin:** Existing repository-local skill.

**Location:** `.codex/skills/prompt-generator/SKILL.md`.

**Status:** Current.

## skill-generator

**Purpose:** Analyze a requested software-system area and create or refresh an approved repository-local modular set of coding skills.

**Activates:** Generating skills for a named directory, layer, subsystem, or capability; refreshing scoped skills after dependency or architecture changes.

**Validated technologies:** Codex skill format; Context7 MCP resolve/query workflow documented in Context7 v1.0.14.

**Technical validation:** 2026-07.

**Related:** skill-creator; prompt-generator.

**Origin:** Generated for this repository from user requirements, current skill-creator guidance, and current Context7 documentation.

**Location:** `.codex/skills/skill-generator/SKILL.md`.

**Status:** Current.
