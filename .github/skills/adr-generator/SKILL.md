---
name: adr-generator
description: >
  **SKILL** - Create clear Architectural Decision Records in `.docs/adr` with explicit rationale, consequences, and alternatives.
  USE FOR: architectural decisions, pattern selection, boundary definition, and integration choices.
  DO NOT USE FOR: general coding work that does not need a recorded decision.
---

# ADR Generator Skill

## Required Inputs

- Decision title
- Context
- Decision
- Alternatives considered
- Stakeholders or owners

## Output Rules

- Save ADRs in `.docs/adr`.
- Use sequential numbering with `adr-NNNN-title-slug.md`.
- Include front matter and explicit sections for Context, Decision, Consequences, Alternatives, Implementation Notes, and References.
- Include both positive and negative consequences.
- Include a do-nothing alternative when relevant.

## Quality Bar

- Clear rationale
- Explicit rejection reasons
- Concrete implementation implications
- Concise, machine-readable markdown