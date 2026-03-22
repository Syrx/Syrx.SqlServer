---
name: syrx-data-access
description: >
  **SKILL** - Implement Syrx repositories and SQL Server data access using the workspace's canonical explicit-SQL pattern.
  USE FOR: repository interfaces, `CommandStrings`, installer wiring, `ICommander<TRepository>` usage, paging, multi-mapping, and Syrx-specific package/configuration guidance.
  DO NOT USE FOR: EF Core, generic ORM guidance, or non-Syrx data access abstractions.
---

# Syrx Data Access Skill

## Role

Use this skill when implementing or reviewing .NET data access that must follow the workspace's Syrx-only rule.

## Canonical Pattern

Follow this sequence:

1. Repository interface
2. Repository implementation
3. `CommandStrings`
4. Syrx installer mapping
5. DI registration

Pattern summary:

`Repository -> Installer -> CommandStrings -> DI Registration`

## Core Concepts

- Syrx with explicit SQL only
- `ICommander<TRepository>` as the primary execution abstraction
- `CommandStrings.cs` for centralized SQL
- `SyrxInstaller.cs` for command-to-method mapping
- SQL Server as the default target database in this workspace

## Implementation Rules

- Use explicit, parameterized SQL only.
- Do not introduce EF Core or alternate ORMs.
- Coalesce query results to empty enumerables where appropriate.
- Use `CancellationToken` on async repository methods.
- Validate public inputs with Syrx guard semantics.
- Prefer soft deletes and explicit column lists.
- Keep repository code focused on persistence mapping, not business logic.

## Typical Repository Shape

```csharp
public interface IUserRepository
{
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> RetrieveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> RetrieveAllAsync(int page = 1, int size = 100, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

```csharp
public sealed class UserRepository : IUserRepository
{
    private readonly ICommander<UserRepository> _commander;

    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander;
    }

    public async Task<User?> RetrieveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var results = await _commander.QueryAsync<User>(new { id }, cancellationToken)
            ?? Enumerable.Empty<User>();

        return results.FirstOrDefault();
    }
}
```

## SQL and CommandStrings

- Keep SQL in `CommandStrings`.
- Match parameter names exactly.
- Avoid `SELECT *`.
- Use paging for all `RetrieveAllAsync` methods.

## Installer and Registration

- Map repository methods explicitly in the Syrx installer.
- Register repository interfaces in DI separately from command mappings.
- Keep connection aliases and connection string configuration centralized.

## Packages

Prefer the matching stable Syrx packages aligned with the solution.

## Advanced Usage

- Use multi-mapping for joined result sets when necessary.
- Use batched result sets when it reduces round-trips cleanly.
- Add optimistic concurrency support with version columns when the domain requires it.

## Quality Bar

- Explicit SQL
- Guarded inputs
- Async + cancellation support
- No business logic in repositories
- Tests use xUnit + Moq only
- No FluentAssertions