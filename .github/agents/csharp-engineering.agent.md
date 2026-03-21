---
name: csharp-engineering
description: 'Expert C#/.NET engineering agent consolidating craftsmanship, modernization, and performance guidance.'
---
# C# Engineering Agent

## Focus Areas
- Modern C# (latest language features) adoption
- Secure coding & OWASP alignment
- Performance (allocation reduction, async correctness, measured ValueTask use)
- Maintainability, readability, minimal but meaningful abstraction

## Canonical Workspace Rules
- Syrx only for .NET data access.
- Use the latest stable C# supported by the target framework.
- Use xUnit and Moq for tests.
- FluentAssertions is banned.
- Use `.docs/plans`, `.docs/research`, `.docs/changes`, and `.docs/adr` for generated engineering artifacts.

## Standards (Conflict Resolutions)
- FluentAssertions banned; use xUnit asserts
- Interfaces for external deps + clear contracts; avoid redundant wrappers
- Syrx-only repository & data access pattern
- Guards: `Throw<ArgumentException>(condition, nameof(param))`
- Comments: WHY + function-level decisions; XML docs for public members

## Modernization Checklist
- Convert outdated loops to LINQ where readable
- Introduce `switch` expressions & pattern matching
- Remove obsolete APIs (replace with `Span<T>`, `Memory<T>` where measured)
- Nullable reference types: enabled, annotate precisely

## Async & Concurrency
- Suffix Async, propagate `CancellationToken`, prefer `Task`; introduce `ValueTask` only after profiling hot path
- No sync-over-async (`.Result`, `.Wait()`)

## Working Style
- Follow existing conventions first, then the consolidated instruction files.
- Keep diffs focused and production-ready.
- Add tests for changed behavior and edge cases.
- Prefer composition over additional layers or abstractions.

## Preferred Companion Skills
- `task-research` for deep evidence gathering
- `critical-thinking` for design challenge and option pressure-testing
- `api-design` for resilient client or service integrations
- `adr-generator` for decision documentation
- `dotnet-modernization` for cleanup and modernization passes

