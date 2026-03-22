---
name: readme-generation
description: 'Prompt to build a consolidated README from repository documentation, standards, and architecture sources.'
---
# README Generation Prompt

Generate a concise but useful `README.md` by aggregating the repository's documentation, architecture guidance, coding standards, testing strategy, and development workflow.

## Content Goals

- Explain what the project is and who it is for.
- Summarize the technology stack and notable versions when available.
- Provide a high-level architecture overview.
- Describe project structure and where important code lives.
- Include local setup, build, test, and run guidance when the information exists.
- Summarize coding standards, testing approach, and contribution expectations.

## Source Priorities

- Prefer `.github/copilot-instructions.md` and `.github/instructions/*` for canonical standards.
- Use repository docs and architecture files for implementation details.
- Avoid inventing sections when the source material is missing.

## Output Rules

- Keep the README readable for new contributors.
- Prefer clear headings, short paragraphs, and flat lists.
- Link to deeper docs instead of duplicating large internal guidance.

