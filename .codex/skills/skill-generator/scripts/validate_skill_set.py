#!/usr/bin/env python3
"""Validate repository-local skills and registry parity without enforcing verbose bodies."""

from __future__ import annotations

import argparse
import re
import sys
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from urllib.parse import unquote

import yaml


NAME_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
REQUIRED_REGISTRY_FIELDS = (
    "Purpose",
    "Activates",
    "Validated technologies",
    "Technical validation",
    "Related",
    "Origin",
    "Location",
    "Status",
)


@dataclass(frozen=True)
class Finding:
    level: str
    location: Path
    message: str


def read_text(path: Path) -> tuple[str, list[Finding]]:
    try:
        return path.read_text(encoding="utf-8-sig"), []
    except (OSError, UnicodeError) as exc:
        return "", [Finding("ERROR", path, f"cannot read UTF-8 content: {exc}")]


def parse_frontmatter(path: Path, text: str) -> tuple[dict[str, object], list[Finding]]:
    findings: list[Finding] = []
    lines = text.splitlines()
    if not lines or lines[0].strip() != "---":
        return {}, [Finding("ERROR", path, "missing opening YAML frontmatter delimiter")]
    try:
        end = next(i for i in range(1, len(lines)) if lines[i].strip() == "---")
    except StopIteration:
        return {}, [Finding("ERROR", path, "missing closing YAML frontmatter delimiter")]

    try:
        values = yaml.safe_load("\n".join(lines[1:end]))
    except yaml.YAMLError as exc:
        return {}, [Finding("ERROR", path, f"invalid YAML frontmatter: {exc}")]
    if not isinstance(values, dict):
        return {}, [Finding("ERROR", path, "YAML frontmatter must be a mapping")]
    return values, findings


def body_after_frontmatter(text: str) -> str:
    match = re.match(r"^---\s*\n.*?\n---\s*\n", text, flags=re.DOTALL)
    return text[match.end() :] if match else text


def validate_links(path: Path, body: str) -> list[Finding]:
    findings: list[Finding] = []
    for target in re.findall(r"(?<!!)\[[^\]]+\]\(([^)]+)\)", body):
        target = target.strip().strip("<>")
        if not target or target.startswith(("#", "http://", "https://", "mailto:")):
            continue
        relative = unquote(target.split("#", 1)[0])
        if not (path.parent / relative).resolve().exists():
            findings.append(Finding("ERROR", path, f"broken relative link: {target}"))
    return findings


def validate_skill(
    path: Path, max_lines: int, max_words: int
) -> tuple[str | None, str, list[Finding]]:
    text, findings = read_text(path)
    if not text:
        return None, "", findings
    values, frontmatter_findings = parse_frontmatter(path, text)
    findings.extend(frontmatter_findings)

    name = values.get("name", "")
    description = values.get("description", "")
    if not isinstance(name, str) or not name:
        findings.append(Finding("ERROR", path, "frontmatter name is required"))
    elif not NAME_RE.fullmatch(name):
        findings.append(Finding("ERROR", path, f"invalid skill name: {name}"))
    elif path.parent.name != name:
        findings.append(
            Finding(
                "ERROR",
                path,
                f"folder '{path.parent.name}' does not match skill name '{name}'",
            )
        )
    if not isinstance(description, str) or not description or description.startswith("["):
        findings.append(
            Finding("ERROR", path, "frontmatter description is missing or placeholder text")
        )
    if re.search(r"\bTODO\b|\[TODO", text, flags=re.IGNORECASE):
        findings.append(Finding("ERROR", path, "contains TODO placeholder text"))

    body = body_after_frontmatter(text)
    line_count = len(text.splitlines())
    word_count = len(re.findall(r"\b[\w.-]+\b", text, flags=re.UNICODE))
    if max_lines and line_count > max_lines:
        findings.append(
            Finding(
                "WARN",
                path,
                f"SKILL.md has {line_count} lines; configured maximum is {max_lines}",
            )
        )
    if max_words and word_count > max_words:
        findings.append(
            Finding(
                "WARN",
                path,
                f"SKILL.md has {word_count} words; configured maximum is {max_words}",
            )
        )
    findings.extend(validate_links(path, body))
    return name or None, body, findings


def discover_skills(root: Path) -> list[Path]:
    return sorted(
        path
        for path in root.rglob("SKILL.md")
        if not any(part.startswith(".") for part in path.relative_to(root).parts[:-1])
    )


def parse_registry(path: Path, text: str) -> tuple[dict[str, str], list[Finding]]:
    findings: list[Finding] = []
    matches = list(re.finditer(r"(?m)^##\s+`?([^`\r\n]+?)`?\s*$", text))
    entries: dict[str, str] = {}
    for index, match in enumerate(matches):
        name = match.group(1).strip()
        end = matches[index + 1].start() if index + 1 < len(matches) else len(text)
        block = text[match.end() : end]
        if not NAME_RE.fullmatch(name):
            findings.append(
                Finding("ERROR", path, f"non-skill level-two registry heading: {name}")
            )
            continue
        if name in entries:
            findings.append(Finding("ERROR", path, f"duplicate registry entry: {name}"))
            continue
        entries[name] = block

        fields = {
            field: re.search(
                rf"(?m)^\*\*{re.escape(field)}:\*\*\s*(.+?)\s*$", block
            )
            for field in REQUIRED_REGISTRY_FIELDS
        }
        for field, field_match in fields.items():
            if not field_match:
                findings.append(
                    Finding(
                        "ERROR",
                        path,
                        f"registry entry '{name}' is missing field: {field}",
                    )
                )
        validation = fields.get("Technical validation")
        if validation and not re.search(
            r"\b20\d{2}-(?:0[1-9]|1[0-2])\b", validation.group(1)
        ):
            findings.append(
                Finding(
                    "ERROR",
                    path,
                    f"registry entry '{name}' has invalid technical validation month",
                )
            )
        location = fields.get("Location")
        if location:
            value = location.group(1).strip().rstrip(".").strip().strip("`")
            expected = f".codex/skills/{name}/SKILL.md"
            if value.replace("\\", "/") != expected:
                findings.append(
                    Finding(
                        "ERROR",
                        path,
                        f"registry entry '{name}' location must be `{expected}`",
                    )
                )
    return entries, findings


def duplicate_paragraph_findings(
    root: Path, bodies: dict[str, str], min_words: int
) -> list[Finding]:
    occurrences: dict[str, set[str]] = defaultdict(set)
    display: dict[str, str] = {}
    for name, body in bodies.items():
        for paragraph in re.split(r"\n\s*\n", body):
            normalized = re.sub(r"\s+", " ", paragraph.strip())
            if normalized.startswith("#"):
                continue
            if len(re.findall(r"\b\w+\b", normalized)) < min_words:
                continue
            key = normalized.casefold()
            occurrences[key].add(name)
            display[key] = normalized

    findings: list[Finding] = []
    for key, names in sorted(
        occurrences.items(), key=lambda item: (-len(item[1]), item[0])
    ):
        if len(names) < 3:
            continue
        excerpt = display[key][:100] + ("…" if len(display[key]) > 100 else "")
        findings.append(
            Finding(
                "WARN",
                root,
                f"paragraph repeated in {len(names)} skills "
                f"({', '.join(sorted(names))}): {excerpt}",
            )
        )
    return findings


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Validate SKILL.md files and INDEX.md registry parity."
    )
    parser.add_argument("--root", type=Path, required=True, help="Skill collection root")
    parser.add_argument(
        "--registry", type=Path, help="Registry path; defaults to <root>/INDEX.md"
    )
    parser.add_argument(
        "--max-lines",
        type=int,
        default=500,
        help="Warn above this line count; 0 disables",
    )
    parser.add_argument(
        "--max-words",
        type=int,
        default=1200,
        help="Warn above this word count; 0 disables",
    )
    parser.add_argument(
        "--check-duplicates",
        action="store_true",
        help="Warn about exact paragraphs repeated in at least three skills",
    )
    parser.add_argument(
        "--duplicate-min-words",
        type=int,
        default=20,
        help="Minimum repeated paragraph size",
    )
    args = parser.parse_args()

    root = args.root.expanduser().resolve()
    registry = (args.registry or root / "INDEX.md").expanduser().resolve()
    findings: list[Finding] = []
    if not root.is_dir():
        print(f"ERROR {root}: skill collection root does not exist", file=sys.stderr)
        return 1

    names: dict[str, Path] = {}
    bodies: dict[str, str] = {}
    skill_paths = discover_skills(root)
    if not skill_paths:
        print(f"ERROR {root}: no SKILL.md files found", file=sys.stderr)
        return 1
    for skill_path in skill_paths:
        name, body, skill_findings = validate_skill(
            skill_path, args.max_lines, args.max_words
        )
        findings.extend(skill_findings)
        if name:
            if name in names:
                findings.append(
                    Finding(
                        "ERROR",
                        skill_path,
                        f"duplicate skill name; also found at {names[name]}",
                    )
                )
            else:
                names[name] = skill_path
                bodies[name] = body

    registry_text, registry_read_findings = read_text(registry)
    findings.extend(registry_read_findings)
    registry_entries: dict[str, str] = {}
    if registry_text:
        registry_entries, registry_findings = parse_registry(registry, registry_text)
        findings.extend(registry_findings)
    for name in sorted(set(names) - set(registry_entries)):
        findings.append(Finding("ERROR", registry, f"missing registry entry for: {name}"))
    for name in sorted(set(registry_entries) - set(names)):
        findings.append(Finding("ERROR", registry, f"orphan registry entry for: {name}"))

    if args.check_duplicates:
        findings.extend(
            duplicate_paragraph_findings(root, bodies, args.duplicate_min_words)
        )

    for finding in findings:
        print(f"{finding.level} {finding.location}: {finding.message}")
    errors = sum(finding.level == "ERROR" for finding in findings)
    warnings = sum(finding.level == "WARN" for finding in findings)
    print(f"Checked {len(skill_paths)} skill(s): {errors} error(s), {warnings} warning(s).")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
