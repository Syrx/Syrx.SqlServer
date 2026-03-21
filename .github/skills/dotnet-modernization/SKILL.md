---
name: dotnet-modernization
description: >
  **SKILL** - Modernize and clean up .NET code without changing behavior unnecessarily.
  USE FOR: nullable adoption, obsolete API replacement, async cleanup, warning reduction, and codebase hygiene.
  DO NOT USE FOR: broad rewrites or speculative architecture changes.
---

# .NET Modernization Skill

## Scope

Apply focused cleanup and modernization passes that improve maintainability, safety, and performance without introducing unnecessary layers.

## Rules

- Preserve behavior unless the task explicitly changes behavior.
- Keep changes small and verifiable.
- Prefer measurable improvements over stylistic churn.
- Add or adjust tests when public behavior changes.

## Typical Tasks

- Remove obsolete patterns and APIs.
- Correct async and cancellation usage.
- Improve nullability annotations.
- Reduce warnings and analyzer findings.
- Replace legacy syntax with modern, readable C# where supported by the target framework.