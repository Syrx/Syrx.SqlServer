---
name: data-access-and-syrx
description: 'Definitive Syrx 2.4.1 repository and SQL Server usage instructions.'
applyTo: '**/*.cs'
---
# Data Access & Syrx Instructions

## Mandatory Pattern
Interface → Implementation → `CommandStrings` → Installer → DI registration.

### Layer Responsibilities
| Layer | Responsibility | Contains |
|-------|----------------|----------|
| Domain | Business rules & invariants | Aggregates, Value Objects, Domain Events |
| Repository | Persistence mapping only | CRUD methods, SQL mapping helpers |
| Application Service | Orchestrates transactions & domain operations | Use cases, coordination |
| Infrastructure | Cross-cutting (logging, connection factory) | Diagnostics, policies |

## CommandStrings
Centralize all SQL. Explicit column lists; soft deletes preferred.

### Structure & Naming
- One static partial class per aggregate: `public static partial class CommandStrings { public static class Order { ... } }`
- Verb + Target pattern: `GetById`, `Insert`, `UpdateStatus`, `SoftDelete`.
- No string interpolation; parameter tokens only (e.g., `@Id`).

### Example
```csharp
public static partial class CommandStrings
{
	public static class Order
	{
		public const string GetById = "SELECT Id, CustomerId, Status, Version FROM dbo.Orders WHERE Id = @Id AND IsDeleted = 0";
		public const string Upsert = "MERGE dbo.Orders AS tgt USING (SELECT @Id AS Id) AS src ON tgt.Id = src.Id WHEN MATCHED THEN UPDATE SET CustomerId=@CustomerId, Status=@Status, Version=Version+1 WHEN NOT MATCHED THEN INSERT (Id, CustomerId, Status, Version) VALUES (@Id, @CustomerId, @Status, 0);";
		public const string SoftDelete = "UPDATE dbo.Orders SET IsDeleted = 1, Version = Version + 1 WHERE Id = @Id";
		public const string ListPaged = "SELECT Id, CustomerId, Status, Version FROM dbo.Orders WHERE IsDeleted=0 ORDER BY CreatedOn DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
	}
}
```

### Soft Delete Rules
- Always include `IsDeleted = 0` in retrieval queries.
- Consider filtered index on `IsDeleted`.

## Guard & Null Safety
Validate inputs with Syrx guard; coalesce query results: `await _commander.QueryAsync<T>(...) ?? Enumerable.Empty<T>();`

Use global `using static Syrx.Validation.Contract;` in project.

### Example Guard in Repository
```csharp
public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
{
	Throw<ArgumentOutOfRangeException>(id != Guid.Empty, "Id required");
	var data = await _commander.QueryAsync<OrderData>(CommandStrings.Order.GetById, new { Id = id }, ct);
	return data?.ToDomain();
}
```

## Performance
Use paging (`OFFSET/FETCH`), avoid SELECT *, profile before introducing multi-mapping.

### Performance Checklist
- Explicit column lists only.
- Appropriate covering indexes (consider INCLUDE for non-key columns).
- Avoid N+1: batch queries or multi-mapping.
- Measure query duration & row count (log threshold > 200ms).
- Prefer table-valued parameters for bulk operations.
- Parameter sniffing issues: use OPTIMIZE FOR UNKNOWN only after evidence; avoid premature hints.

### Multi-Mapping Patterns

Syrx supports two distinct patterns for handling complex queries: **Multi-mapping** (row-by-row object composition) and **Multiple Result Sets** (collection-to-collection processing).

#### A. Multi-Mapping (Row-by-Row Composition)
Use when JOIN returns related rows that need to be composed into objects with relationships.

**SQL (explicit columns + split-on point):**
```sql
SELECT o.Id, o.CustomerId, o.Status, o.Version,
       l.Id, l.ProductId, l.Quantity, l.UnitPrice
FROM dbo.Orders o
LEFT JOIN dbo.OrderLines l ON l.OrderId = o.Id AND l.IsDeleted = 0
WHERE o.Id = @Id AND o.IsDeleted = 0;
```

**Repository (using multi-map signature with Func<T1, T2, TResult>):**
```csharp
public async Task<Order?> GetOrderWithLinesAsync(Guid id, CancellationToken ct)
{
    var orders = await _commander.QueryAsync<Order, OrderLine, Order>(
        (order, line) =>
        {
            // Syrx invokes this per row; handle collection aggregation
            if (line?.Id != Guid.Empty)
            {
                order.Lines ??= new List<OrderLine>();
                if (!order.Lines.Any(l => l.Id == line.Id))
                    order.Lines.Add(line);
            }
            return order;
        },
        new { Id = id },
        ct);
    
    return orders.FirstOrDefault();
}
```

**Rules:**
- Mapping function invoked per row from result set.
- Handle duplicate parent objects (group/deduplicate in mapping function or post-process).
- Use `splitOn` configuration to tell Dapper where child columns begin (default: `Id`).
- Nullable child properties indicate LEFT JOIN absence.

#### B. Multiple Result Sets (Collection-to-Collection Mapping)
Use when SQL returns multiple independent SELECT statements; process entire collections.

**SQL (multiple SELECT statements):**
```sql
SELECT Id, CustomerId, Status, Version FROM dbo.Orders WHERE Id=@Id AND IsDeleted=0;
SELECT Id, OrderId, ProductId, Quantity, UnitPrice FROM dbo.OrderLines WHERE OrderId=@Id AND IsDeleted=0;
```

**Repository (using Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TResult>>):**
```csharp
public async Task<Order?> GetOrderWithLinesMultiAsync(Guid id, CancellationToken ct)
{
    var results = await _commander.QueryAsync<OrderData, OrderLineData, Order>(
        (orders, lines) =>
        {
            var orderData = orders.FirstOrDefault();
            if (orderData is null) return Enumerable.Empty<Order>();
            
            return new[] { OrderMaterializer.Materialize(orderData, lines) };
        },
        new { Id = id },
        ct);
    
    return results.FirstOrDefault();
}
```

**Rules:**
- Mapping function receives full collections (not individual rows).
- SQL must return multiple result sets (batched SELECTs or stored procedure).
- Always validate first collection before processing subsequent ones.
- Return `IEnumerable<TResult>` (typically single-item or empty).

#### Pattern Selection Matrix
| Scenario | Pattern | Signature | SQL Structure |
|----------|---------|-----------|---------------|
| Parent + children via JOIN | Multi-mapping | `Func<T1, T2, TResult>` | Single SELECT with JOIN |
| Independent queries batch | Multiple Result Sets | `Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TResult>>` | Multiple SELECT statements |
| Moderate child counts | Multi-mapping | Row-by-row | JOIN with splitOn |
| Large child collections | Multiple Result Sets | Collection-based | Separate SELECTs |

#### Anti-Patterns (Avoid)
| Pattern | Problem | Corrective Action |
|---------|---------|------------------|
| Two separate `QueryAsync` calls | Extra round trips; inconsistency risk | Use multi-map or multiple result sets |
| Mutating aggregate per row unsafely | Race conditions; partial state | Deduplicate in mapping or post-process |
| SELECT * in joins | Schema coupling; unnecessary I/O | Explicit column lists |
| N+1 queries (loop calling repo) | Latency explosion | Batch in single multi-map/multi-result-set |

#### Materializer Isolation
Keep domain reconstruction logic separate:
```csharp
public static class OrderMaterializer
{
    public static Order Materialize(OrderData data, IEnumerable<OrderLineData> lines)
    {
        var order = Order.CreateExisting(data.Id, data.CustomerId, data.Status, data.Version);
        foreach (var l in lines)
            order.AddLineExisting(l.Id, l.ProductId, l.Quantity, l.UnitPrice);
        return order;
    }
}
```

#### Configuration Example (splitOn for multi-mapping)
```json
{
  "Commands": {
    "GetOrderWithLinesAsync": {
      "CommandText": "SELECT o.Id, o.CustomerId, o.Status, o.Version, l.Id, l.ProductId, l.Quantity, l.UnitPrice FROM dbo.Orders o LEFT JOIN dbo.OrderLines l ON l.OrderId = o.Id WHERE o.Id = @Id",
      "ConnectionAlias": "Default",
      "SplitOn": "Id"
    }
  }
}
```

### High-Frequency Query Optimization
- Benchmark with `dotnet-counters` (monitor `System.Runtime` allocations, GC).
- Consider caching immutable lookup tables (in-memory with refresh interval).

### Concurrency Control
- Include `Version` column; compare before update.
```sql
UPDATE dbo.Orders SET Status=@Status, Version=Version+1 WHERE Id=@Id AND Version=@ExpectedVersion;
```
If affected rows = 0 → throw concurrency exception.

## Testing
Mock `ICommander<T>`; do not connect to DB in unit tests.

### Repository Unit Test Example
```csharp
using static XUnit.Assert;

namespace Samples.Repositories.OrderRepositoryTests 
{
    public class OrderRepositoryTestsFixture
    {
        public IOrderRepository Repository { get; }

        public OrderRepositoryTestsFixture()
        {
            var commander = SetupCommander();
            Repository = new OrderRepository(commander.Object);
        }

        private Mock<ICommander<IOrderRepository>> SetupCommander()
        {
            var commander = new Mock<ICommander<IOrderRepository>>();

            commander.Setup(c => c.QueryAsync<OrderData>(
                    It.IsAny<object>(), 
                    It.IsAny<CancellationToken>())).ReturnsAsync((OrderData?)null);

            return commander;        
        }
    }

    public class GetAsync(OrderRepositoryTestsFixture fixture)
    {
        private readonly IOrderRepository _repository = fixture.Repository;

        [Fact]
        public async Task ReturnsNullWhenNotFound()
        {
            var result = await _repository.GetAsync(Guid.NewGuid());
            Null(result);
        }
    }
}
```

### Integration Testing Guidance
- Use ephemeral test database (Docker SQL Server) seeded per test collection.
- Wrap tests in transaction & rollback after test for isolation.
- Validate SQL plan regression only for critical queries (capture execution statistics).

### Test Naming Pattern
`GetByIdReturnsNullWhenMissing`, `UpsertIncrementsVersionOnUpdate`.

### Anti-Patterns (Avoid)
| Anti-Pattern | Risk | Replacement |
|--------------|------|-------------|
| SELECT * | Schema drift, extra I/O | Explicit columns |
| Dynamic table names | Injection vector | Static CommandStrings |
| String concatenated SQL | Injection risk | Parameterized constants |
| Business logic in repository | Leaky architecture | Move to domain/application layer |
| Unbounded result sets | Memory pressure | Paging with OFFSET/FETCH |

## Transactions
Application service coordinates units of work.
```csharp
using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
await orderRepository.SaveAsync(order, ct);
// publish events after commit
scope.Complete();
```
Keep scopes small; never wrap read-only queries unless part of consistent snapshot requirement.

### Event Dispatch Timing
- Persist aggregate first.
- Dispatch domain events post-commit to avoid phantom publishes.

## Installer Pattern
```csharp
public static class RepositoryInstaller
{
	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		services.AddScoped<IOrderRepository, OrderRepository>();
		return services;
	}
}
```

## Mapping DTOs
```csharp
public sealed record OrderData(Guid Id, Guid CustomerId, string Status, int Version)
{
	public Order ToDomain(IEnumerable<OrderLineData> lines) => OrderMaterializer.Materialize(this, lines);
}
```
Keep materialization isolated (`OrderMaterializer`) for testability.

## Observability
- Log command name, duration, row count.
- Tag slow queries: `SlowQuery=true` in structured log.
- Metrics: count executions, average duration, p95 latency.

## Security Guidelines
- Least privilege DB user (no DDL for runtime).
- Parameterization mandatory; never concatenate inputs.
- Avoid exposing raw SQL errors to callers; map to safe domain exceptions.

## Cheat Sheet
| Topic | Rule |
|-------|------|
| Column Lists | Always explicit |
| Guards | `using static` + guard at entry |
| Concurrency | Version column optimistic |
| Soft Delete | Always filter IsDeleted=0 |
| Paging | OFFSET/FETCH for large sets |
| Multi-Mapping | Fetch parts then compose |
| Transactions | Short-lived; commit before events |
| Testing | Mock commander; integration uses ephemeral DB |

## Review Checklist
- No business rules in repository methods.
- All SQL centralized & parameterized.
- Guards for public method inputs present.
- Concurrency handled on updates.
- Logging & metrics instrumentation configured.
- Soft delete adherence verified.

## Evolution Strategy
- Quarterly audit for unused queries.
- ADR for introducing caching or new indexing strategies.
- Benchmark changes before merging performance PRs.

