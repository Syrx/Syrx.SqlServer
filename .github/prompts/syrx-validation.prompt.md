---
name: syrx-validation
agent: agent
description: 'Prompt for applying Syrx guard validation patterns consistently.'
---

# Validation Pattern

Your goal is to help me use the **Syrx.Validation.Contract.Throw<TException>** for all input validation. 
You can consult the source for the **Syrx.Validation.Contract** type at https://github.com/Syrx/Syrx.Validation/blob/main/src/Syrx.Validation/Contract.cs
You can install from nuget.org using the package Id **Syrx.Validation**. 
Prefer transitive references if possible. 

## Workspace Rules

- Use Syrx guard semantics consistently at public boundaries.
- Preserve success-condition semantics: `true` means proceed, `false` throws.
- Prefer precise exception types.
- Keep validation at method entry before state mutation.
- Align recommendations with the consolidated validation and C# instruction files.

```csharp
using static Syrx.Validation.Contract;

public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
{
    // Guard against null - condition is the SUCCESS condition
    // If user != null evaluates to TRUE, all is good
    // If user != null evaluates to FALSE (user is null), throw exception
    Throw<ArgumentNullException>(user != null, nameof(user));
    
    return await _commander.ExecuteAsync(user, cancellationToken) ? user : null!;
}

public async Task<User?> RetrieveByEmailAsync(string email, CancellationToken cancellationToken = default)
{
    // Defensive check - throw exception for invalid input
    // This exception to be handled by the caller for better orchestration
    Throw<ArgumentException>(!string.IsNullOrWhiteSpace(email), nameof(email));
    
    var users = await _commander.QueryAsync<User>(new { email }, cancellationToken);
    return users.FirstOrDefault();
}

public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
{
    // Defensive check - throw exception for invalid input
    // This exception to be handled by the caller for better orchestration
    Throw<ArgumentException>(!string.IsNullOrWhiteSpace(email), nameof(email));
    
    var results = await _commander.QueryAsync<bool>(new { email }, cancellationToken);
    return results.FirstOrDefault();
}
```

**Key Principle:** The `condition` parameter in `Throw<TException>(condition, message)` is the **SUCCESS condition**:
- If `condition == true`: All is good, no exception thrown
- If `condition == false`: Throw the exception with the provided message

**Rationale for Throwing in Queries:**  
Exceptions allow the caller (Application/API layer) to handle validation failures consistently via middleware, providing better orchestration and centralized error handling. This follows the fail-fast principle and prevents invalid data from propagating through the system.

### Contract.Throw Overloads

```csharp
// Simple form with parameter name
Throw<ArgumentException>(value != null, nameof(value));

// With custom message
Throw<ArgumentException>(id != Guid.Empty, "User ID cannot be empty");

// With formatted message
Throw<ArgumentException>(count > 0, "Count must be positive, got {0}", count);

// With inner exception
Throw<InvalidOperationException>(result != null, "Operation failed", innerException);

// With exception factory
Throw<CustomException>(isValid, () => new CustomException("Custom logic"));
```