#!/usr/bin/env python3
"""Validate generated skill structure and registry coverage using the standard library."""

from __future__ import annotations

import argparse
import re
import sys
from dataclasses import dataclass
from pathlib import Path


NAME_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
RECOMMENDED_SECTIONS = (
    "scope",
    "required context",
    "architecture",
    "workflow",
    "implementation",
    "conventions",
    "decision",
    "anti-pattern",
    "related skills",
    "repository references",
    "verification",
    "definition of done",
    "context7 fallback",
    "validated technologies",
)


@dataclass(frozen=True)
class Finding:
    level: str
    location: Path
    message: str


def parse_frontmatter(path: Path) -> tuple[dict[str, str], list[Finding]]:
    findings: list[Finding] = []
    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeError) as exc:
        return {}, [Finding("ERROR", path, f"cannot read UTF-8 content: {exc}")]

    lines = text.splitlines()
    if not lines or lines[0].strip() != "---":
        return {}, [Finding("ERROR", path, "missing opening YAML frontmatter delimiter")]

    try:
        end = next(i for i in range(1, len(lines)) if lines[i].strip() == "---")
    except StopIteration:
        return {}, [Finding("ERROR", path, "missing closing YAML frontmatter delimiter")]

    values: dict[str, str] = {}
    for line_number, line in enumerate(lines[1:end], start=2):
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        match = re.match(r"^([A-Za-z0-9_-]+):\s*(.*)$", line)
        if not match:
            findings.append(
                Finding("ERROR", path, f"unsupported frontmatter syntax on line {line_number}")
            )
            continue
        key, value = match.groups()
        if key in values:
            findings.append(Finding("ERROR", path, f"duplicate frontmatter key: {key}"))
        values[key] = value.strip().strip('"\'')

    return values, findings


def validate_skill(path: Path) -> tuple[str | None, list[Finding]]:
    findings: list[Finding] = []
    values, frontmatter_findings = parse_frontmatter(path)
    findings.extend(frontmatter_findings)

    extra_keys = sorted(set(values) - {"name", "description"})
    if extra_keys:
        findings.append(
            Finding("ERROR", path, f"frontmatter contains unsupported keys: {', '.join(extra_keys)}")
        )

    name = values.get("name", "")
    description = values.get("description", "")
    if not name:
        findings.append(Finding("ERROR", path, "frontmatter name is required"))
    elif not NAME_RE.fullmatch(name):
        findings.append(Finding("ERROR", path, f"invalid skill name: {name}"))
    elif path.parent.name != name:
        findings.append(
            Finding("ERROR", path, f"folder '{path.parent.name}' does not match skill name '{name}'")
        )

    if not description or description.startswith("["):
        findings.append(Finding("ERROR", path, "frontmatter description is missing or placeholder text"))

    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeError):
        return name or None, findings

    if re.search(r"\bTODO\b|\[TODO", text, flags=re.IGNORECASE):
        findings.append(Finding("ERROR", path, "contains TODO placeholder text"))

    line_count = len(text.splitlines())
    if line_count > 500:
        findings.append(
            Finding("WARN", path, f"SKILL.md has {line_count} lines; prefer fewer than 500")
        )

    headings = {
        match.group(1).strip().lower()
        for match in re.finditer(r"^#{2,6}\s+(.+?)\s*$", text, flags=re.MULTILINE)
    }
    for expected in RECOMMENDED_SECTIONS:
        if not any(expected in heading for heading in headings):
            findings.append(
                Finding("WARN", path, f"recommended generated-skill section not found: {expected}")
            )

    if "context7" not in text.lower():
        findings.append(Finding("WARN", path, "does not mention a Context7 fallback"))
    if not re.search(r"\b20\d{2}-(?:0[1-9]|1[0-2])\b", text):
        findings.append(Finding("WARN", path, "technical validation month (YYYY-MM) not found"))

    return name or None, findings


def registry_contains(registry_text: str, name: str) -> bool:
    pattern = re.compile(rf"^##\s+`?{re.escape(name)}`?\s*$", flags=re.MULTILINE)
    return bool(pattern.search(registry_text))


def discover_skills(root: Path) -> list[Path]:
    return sorted(
        path
        for path in root.rglob("SKILL.md")
        if not any(part.startswith(".") for part in path.relative_to(root).parts[:-1])
    )


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Validate generated SKILL.md files and INDEX.md registry coverage."
    )
    parser.add_argument("--root", type=Path, required=True, help="Skill collection root")
    parser.add_argument("--registry", type=Path, help="Registry path; defaults to <root>/INDEX.md")
    args = parser.parse_args()

    root = args.root.expanduser().resolve()
    registry = (args.registry or root / "INDEX.md").expanduser().resolve()
    findings: list[Finding] = []

    if not root.is_dir():
        print(f"ERROR {root}: skill collection root does not exist", file=sys.stderr)
        return 1

    skill_paths = discover_skills(root)
    if not skill_paths:
        print(f"ERROR {root}: no SKILL.md files found", file=sys.stderr)
        return 1

    names: dict[str, Path] = {}
    for skill_path in skill_paths:
        name, skill_findings = validate_skill(skill_path)
        findings.extend(skill_findings)
        if name:
            if name in names:
                findings.append(
                    Finding("ERROR", skill_path, f"duplicate skill name; also found at {names[name]}")
                )
            else:
                names[name] = skill_path

    registry_text = ""
    if not registry.is_file():
        findings.append(Finding("ERROR", registry, "skill registry does not exist"))
    else:
        try:
            registry_text = registry.read_text(encoding="utf-8")
        except (OSError, UnicodeError) as exc:
            findings.append(Finding("ERROR", registry, f"cannot read UTF-8 content: {exc}"))

    if registry_text:
        for name in sorted(names):
            if not registry_contains(registry_text, name):
                findings.append(Finding("ERROR", registry, f"missing registry entry for: {name}"))

    for finding in findings:
        print(f"{finding.level} {finding.location}: {finding.message}")

    errors = sum(finding.level == "ERROR" for finding in findings)
    warnings = sum(finding.level == "WARN" for finding in findings)
    print(
        f"Checked {len(skill_paths)} skill(s): {errors} error(s), {warnings} warning(s)."
    )
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())

