---
name: validation-and-guards
description: 'Standardized Syrx guard usage and validation rationale.'
applyTo: '**/*.cs'
---
# Validation & Guards

## Pattern
`Throw<TException>(successCondition, message)` where `successCondition == true` means proceed; false throws.

## Examples
```csharp
Throw<ArgumentNullException>(user != null, nameof(user));
Throw<ArgumentException>(!string.IsNullOrWhiteSpace(email), nameof(email));
```

## Rationale
Fail-fast, centralizes validation semantics, consistent exception taxonomy.

