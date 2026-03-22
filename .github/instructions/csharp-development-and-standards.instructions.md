---
name: csharp-development-and-standards
description: 'Consolidated C# development, style, and engineering standards.'
applyTo: '**/*.cs'
---
# C# Development & Standards

## Language & Tooling
- Always target latest stable C#; update `LangVersion` when feasible.
- Enable nullable reference types; annotate strictly.
 - Treat compiler warnings as errors (enable `TreatWarningsAsErrors`).
 - Enable analyzers: StyleCop (style), Roslyn analyzers (safety), Security analyzers (credential leakage).
 - Use `global using` for ubiquitous framework namespaces; avoid overuse.
 - Prefer file-scoped namespaces.
 - Utilize raw string literals for multi-line JSON/SQL only when parameterization remains intact.

### Feature Adoption Matrix (Assess Quarterly)
| Feature | Adopt? | Criteria | Migration Pattern |
|---------|--------|---------|-------------------|
| Records | Yes | DTOs immutable, value semantics | Replace POCO + equality methods with `record` |
| Primary Constructors | Conditional | Simplicity vs DI container needs | Refactor small data-centric classes |
| Pattern Matching Enhancements | Yes | Improves clarity vs multiple `if` chains | Replace type + null checks |
| Raw String Literals | Yes | Multi-line readability of test assets/config | Replace verbatim + concatenations |
| `Span<T>`/Memory APIs | Conditional | Proven profiler hotspot & allocation pressure | Isolate in internal perf layer |
| `ValueTask` | Conditional | Async method awaited synchronously frequently; high call volume | Replace `Task` + add benchmark |

### Modernization Workflow
1. Inventory legacy constructs (manual equality, explicit tuple types, verbose null checks).
2. Benchmark allocation & CPU hotspots (dotnet-trace, PerfView).
3. Create ADR for risky transformations (e.g., introducing `Span<T>`).
4. Apply automated refactors (Rider/VS analyzers) in small PRs.
5. Run test + performance delta (baseline vs candidate).
6. Tag change set with performance label in PR description.

## Naming & Structure
- PascalCase for types/methods; camelCase locals; interfaces prefixed `I`.
- Namespace organization by feature/domain.
- Follow `.editorconfig` and local project conventions first.

## Comments & Documentation
- Comments should explain WHY and design rationale when needed.
- XML docs for all public members; use `<see cref>` / `<example>` appropriately.
 - Include rationale for non-trivial exceptions & guard decisions.
 - Avoid repeating obvious implementation details; focus on intent & constraints.

## Validation (Global Rule)
Use `Syrx.Validation.Contract.Throw<T>(successCondition, message)` for all parameter guards.

### Guard Taxonomy
| Scenario | Guard Pattern | Exception |
|----------|---------------|-----------|
| Null reference | `Throw<ArgumentNullException>(obj is not null, nameof(obj))` | `ArgumentNullException` |
| Empty string | `Throw<ArgumentException>(!string.IsNullOrWhiteSpace(name), "Name required")` | `ArgumentException` |
| Range check | `Throw<ArgumentOutOfRangeException>(value >= min && value <= max, "Value out of range")` | `ArgumentOutOfRangeException` |
| Enum validity | `Enum.IsDefined(typeof(TEnum), value)` then throw | `ArgumentOutOfRangeException` |
| Collection non-empty | `Throw<ArgumentException>(items.Any(), "Items required")` | `ArgumentException` |
| Business rule failure | Domain-specific condition | Custom domain exception |

All guards occur at method entry; no partial state mutation before validation completes.

## Async
- Suffix Async; propagate `CancellationToken`.
- Use `Task`; introduce `ValueTask` only for validated high-frequency hot paths.

### Async Excellence Checklist
- Avoid `async void` (except event handlers).
- Do not block (`.Result`, `.Wait()`, `Task.Run` around sync code) — prevents deadlocks.
- Leverage `ConfigureAwait(false)` in library/internal layers (not UI).
- Cancellation token required in public async APIs.
- Benchmark candidate for `ValueTask` (Track call count & allocation via dotnet-counters).

### ValueTask Decision Flow
1. Is method frequently called (>100k/sec in perf tests)? If no → keep `Task`.
2. Does method complete synchronously often (>80%)? If no → keep `Task`.
3. Profiling shows allocation benefit? If yes → migrate & document.
4. Ensure consumer patterns (multiple awaits, `Task.WhenAll`) do not misuse `ValueTask`.

## Data Access
Syrx-only repositories; no EF Core or other ORMs.

### Dependency Rules
- Do not introduce EF Core, FluentValidation, or alternate ORM abstractions.
- Keep abstractions minimal and justified by testing, boundaries, or external dependencies.

### Repository Pattern (Syrx-Only) Guidance
- One repository per aggregate root boundary.
- Avoid generic catch-all repository abstractions.
- Centralize SQL fragments via `CommandStrings`; ensure parameter tokens not interpolated.
- Transactions orchestrated at service/application layer; repositories remain atomic.
- Map multi-result sets via structured tuples — keep mapping pure.

## Testing Interface Guidance
Interfaces required for external dependencies & test seams; avoid redundant abstractions around already abstract frameworks.

### Interface Creation Rules
- Create interface when mocking required for unit test and dependency is concrete.
- Do NOT create interface for data-only records or simple configuration objects.
- Consolidate interface members around cohesive responsibility (ISP) — no placeholder methods.

### Example Bad vs Good
Bad:
```csharp
public interface IFooHelper { Task LogAsync(string message); Task<int> ParseAsync(string s); }
```
Good:
```csharp
public interface ILogWriter { Task WriteAsync(string message, CancellationToken ct); }
public interface IStringParser { Task<int> ParseAsync(string value, CancellationToken ct); }
```

## Security & OWASP
- Deny by default access control
- No secrets in code (env or secret store)
- Parameterized SQL only (Syrx explicit SQL)

### Secure Coding Embeds
- Always validate external inputs (length, range, format) before data access.
- Use structured logging (no PII in messages; redact secrets).
- Apply output encoding in any HTML generation contexts.
- Prefer composition over inheritance for security-sensitive components.

## Prohibited
- FluentAssertions
- Sync-over-async calls
 - Reflection-based performance micro-optimizations without benchmark
 - Implicit culture-dependent parsing (specify `CultureInfo.InvariantCulture`)
 - Silent catch blocks (must log & rethrow or wrap)

## Practical Defaults
- Prefer file-scoped namespaces.
- Prefer modern C# features when they improve clarity and are supported by the target framework.
- Add tests for changed public behavior and important edge cases.
- Keep documentation aligned with actual behavior and naming.

## Performance Patterns
- Minimize allocations in hot paths (avoid LINQ in tight loops; use `for`).
- Use `ArrayPool<T>` only with clear lifetime & zeroing requirements.
- Profile before optimizing (avoid speculative micro-optimizations).
- Prefer structs only when size <= 16 bytes and copying cheaper than heap.

## Code Review Checklist (Condensed)
- Clear guard usage & exceptions mapped.
- Async patterns: no blocking, proper cancellation.
- No banned dependencies or patterns.
- Documentation: WHY present for complex logic.
- Security: parameterized queries, no secrets, input validation.
- Performance: hotspots justified by benchmark if optimized.

## Example Modernization Diff
```diff
- if (user != null && user.IsActive == true) { }
+ if (user is { IsActive: true }) { }
```

```diff
- var sb = new StringBuilder(); sb.AppendLine("{\"x\":1}");
+ var json = """
{"x":1}
""";
```

## Benchmark Template
```bash
dotnet build -c Release
dotnet run -c Release --project benchmarks/YourBenchmarks
```

Record before/after metrics (allocs, mean, p95) in ADR.

