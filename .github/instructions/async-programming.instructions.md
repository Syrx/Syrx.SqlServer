---
name: async-programming
description: 'Consolidated async/await and concurrency guidance with ValueTask policy.'
applyTo: '**/*.cs'
---
# Async Programming Guidelines

## Method Signatures
Suffix Async; return `Task`/`Task<T>`; consider `ValueTask` only after profiling.

## Cancellation
Accept & propagate `CancellationToken`.

## Pitfalls
No `.Result`/`.Wait()`; no fire-and-forget; avoid unobserved tasks.

## Performance
Batch parallel tasks with `Task.WhenAll`; prefer streaming with `IAsyncEnumerable<T>` for large sequences.

