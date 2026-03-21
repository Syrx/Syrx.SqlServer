---
name: architecture-ddd-and-domain
description: 'Consolidated DDD, SOLID, and .NET architecture guidance for lean application development.'
applyTo: '**/*.cs'
---
# Architecture, DDD & Domain Guidelines

## Pre-Implementation Analysis
Enumerate aggregates, invariants, domain events; validate SOLID adherence; confirm ubiquitous language consistency before coding.

### Analysis Checklist
- Ubiquitous language glossary drafted & reviewed with domain experts.
- Candidate aggregates listed with invariants per aggregate.
- External systems identified; note integration style (events / ACL / direct API).
- Cross-cutting concerns mapped (security, validation, logging, observability).
- Hot path assumptions listed (later validated via profiling).

### Aggregate Discovery Steps
1. List domain nouns; group by cohesive invariants.
2. Identify transactional boundaries (what must commit atomically?).
3. Detect lifecycle state machine (states + transitions).
4. Validate invariants can be enforced within single aggregate root.
5. If invariants span multiple groups → consider domain service or saga.

## Aggregates & Entities
- Aggregates enforce consistency boundaries.
- Value Objects: immutable; equality by value.

### Aggregate Design Checklist
| Item | Rule |
|------|------|
| Invariants | Encapsulated in methods; never exposed for external enforcement |
| Constructors | Private/protected; use static factory for complex creation |
| Mutations | Methods express business language (e.g., `Ship()`, `Cancel(reason)`) |
| References | Other aggregates referenced by ID only (avoid direct object coupling) |
| Collections | Manage via intent methods (`AddLineItem`, `RemoveLineItem`) |
| Concurrency | Include version field for optimistic concurrency |

### Value Object Pattern
```csharp
using static Syrx.Validation.Contract; // Prefer global using static in project-wide global usings

public sealed record Money(decimal Amount, string Currency)
{
	public static Money Of(decimal amount, string currency)
	{
		Throw<ArgumentOutOfRangeException>(amount >= 0, "Amount must be non-negative");
		Throw<ArgumentException>(!string.IsNullOrWhiteSpace(currency), "Currency required");
		return new Money(amount, currency.ToUpperInvariant());
	}

	public Money Convert(decimal rate, string targetCurrency) => Of(Math.Round(Amount * rate, 2), targetCurrency);
}
```

### Invariant Enforcement Example
```csharp
public class Order
{
	private readonly List<OrderLine> _lines = new();
	public Guid Id { get; }
	public string Status { get; private set; } = "Draft";
	public IReadOnlyCollection<OrderLine> Lines => _lines;

	private Order(Guid id) => Id = id;
	public static Order Create() => new Order(Guid.NewGuid());

	public void AddLine(Product product, int quantity)
	{
		Throw<ArgumentOutOfRangeException>(quantity > 0, "Quantity > 0");
		_lines.Add(new OrderLine(product.Id, quantity, product.Price));
		EnsureTotalNotNegative();
	}

	public void Ship()
	{
		Throw<InvalidOperationException>(Status == "Draft" || Status == "Packed", "Cannot ship from current state");
		Status = "Shipped";
		AddDomainEvent(new OrderShippedEvent(Id));
	}

	private void EnsureTotalNotNegative()
	{
		var total = _lines.Sum(l => l.UnitPrice * l.Quantity);
		Throw<InvalidOperationException>(total >= 0, "Total cannot be negative");
	}

	private readonly List<IDomainEvent> _events = new();
	public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;
	private void AddDomainEvent(IDomainEvent evt) => _events.Add(evt);
}
```

## Domain Events
Capture significant changes; facilitate audit & integration.

### Event Lifecycle
1. Aggregate method executes & invariants validated.
2. Domain event instance created & appended to aggregate event collection.
3. Unit of Work/Repository persists state + dispatches events post-commit.
4. Handlers process event (synchronous or async) using ID only; avoid fetching entire graphs unnecessarily.

### Naming Convention
`<Aggregate><PastTense>` e.g., `OrderPlacedEvent`, `PaymentCapturedEvent`.

### Event Handler Pattern
```csharp
public sealed class OrderShippedHandler : IDomainEventHandler<OrderShippedEvent>
{
	private readonly IShippingGateway _gateway;
	public OrderShippedHandler(IShippingGateway gateway) => _gateway = gateway;
	public async Task HandleAsync(OrderShippedEvent evt, CancellationToken ct)
	{
		await _gateway.NotifyShipmentAsync(evt.OrderId, ct);
	}
}
```

### Event Publishing (In-Process Dispatcher)
```csharp
public interface IDomainEventDispatcher
{
	Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
}
```

### Avoid
- Events for trivial state changes (noise).
- Including large payloads (pass identifiers; fetch details if required).

## Repositories
Syrx explicit SQL only; each aggregate persistence via dedicated repository.

### Repository Structure
| Layer | Responsibility |
|-------|----------------|
| Repository | Convert aggregate to persisted representation & vice versa |
| Application Service | Orchestrates repositories & domain logic |
| Domain | Contains business rules & events |

### Syrx Repository Example (Simplified)
```csharp
public interface IOrderRepository
{
	Task<Order?> GetAsync(Guid id, CancellationToken ct);
	Task SaveAsync(Order order, CancellationToken ct);
}

public sealed class OrderRepository : IOrderRepository
{
	private readonly ICommander<IOrderRepository> _commander;
	public OrderRepository(ICommander<IOrderRepository> commander) => _commander = commander;

	public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
	{
		var cmd = CommandStrings.Order.GetById;
		var data = await _commander.QueryAsync<OrderData>(cmd, new { Id = id }, ct);
		return data?.ToDomain();
	}

	public async Task SaveAsync(Order order, CancellationToken ct)
	{
		// Example: Upsert
		var cmd = CommandStrings.Order.Upsert;
		var dto = OrderData.FromDomain(order);
		await _commander.ExecuteAsync(cmd, dto, ct);
	}
}
```

### Transaction Guidance
- Keep transactions short (write operations only).
- Publish domain events after commit.
- Use optimistic concurrency (version column) — on mismatch raise conflict exception.

### Multi-Mapping Strategy
- Query multiple tables; map to simple DTO set; compose domain aggregate.
- Avoid passing raw data structures past repository boundary.

## Decision Records
Use ADR workflow to document rationale in `.docs/adr`.

### ADR Mandatory Sections
- Context (drivers & constraints)
- Decision (concise statement)
- Consequences (POS/NEG with IDs)
- Alternatives (include "Do Nothing")
- Implementation Notes (monitoring, rollback)
- References (links, tickets)

### Example Consequence IDs
- POS-001 Improved decoupling
- POS-002 Better performance under load
- NEG-001 Increased complexity
- NEG-002 Additional maintenance overhead

### Quality Gate
- Minimum 2 alternatives.
- At least 2 positive & 2 negative consequences.
- Security consideration explicitly stated.

## Anti-Corruption Layer (ACL)
When integrating legacy/external systems:
- Isolate translation logic in dedicated adapter.
- Map external DTO → domain value objects (validate invariants).
- Reject invalid external data early (guards + logging).

### ACL Code Sketch
```csharp
public sealed class LegacyInvoiceAdapter
{
	public Invoice Map(LegacyInvoice dto)
	{
		Throw<ArgumentException>(dto.Total >= 0, "Total must be non-negative");
		var money = Money.Of(dto.Total, dto.Currency);
		return Invoice.Create(dto.Id, money, dto.IssuedOn);
	}
}
```

## Pattern Selection Matrix
| Scenario | Pattern | Notes |
|----------|---------|-------|
| Complex process across aggregates | Saga / Process Manager | Keep domain events orchestration external |
| Need read model optimization | CQRS (read projection) | Separate query model; domain remains rich |
| External integration boundary | Anti-Corruption Layer | Prevent leaking external semantics |
| High rate domain events | Event Batching / Debouncing | Aggregate or consolidate notifications |

## Review Checklist (Architecture PR)
- Aggregate invariants encapsulated (no leaking validation externally).
- Domain events meaningful & named with past tense.
- Repositories do not expose infrastructure types.
- ADR attached or referenced for significant architectural shifts.
- Anti-Corruption layers employed for legacy boundaries.
- Security & performance considerations documented.

## Common Pitfalls & Remedies
| Pitfall | Symptom | Remedy |
|---------|---------|--------|
| Anemic Model | Services contain business logic | Move logic into aggregate methods |
| Overusing Events | Hard to trace flow | Consolidate; audit necessity per event |
| Repository Chatty Calls | Multiple round trips | Introduce bulk/multi-mapping query |
| Tight Coupling | Direct object dependency across aggregates | Use IDs + domain events |

## Evolution Strategy
- Quarterly review of aggregate boundaries & event taxonomy.
- Archive deprecated events with ADR referencing removal.
- Track performance metrics (command latency, event handler failures).

## Observability Hooks
- Emit structured logs on domain event publish (EventName, AggregateId, Version).
- Metrics: count events per type, handler duration percentiles.
- Correlate via trace ID passed through application layer.

## Security Embedding
- Validate input at boundary (DTO → domain) using guards.
- Redact sensitive fields from logs.
- Ensure no direct SQL injection vector (parameterized queries only).

## Glossary Section (Required)

## .NET Layering Rules
- Domain layer contains business rules, invariants, value objects, domain services, and domain events.
- Application layer orchestrates use cases and transaction boundaries.
- Infrastructure layer implements repositories, integrations, and platform concerns.
- Keep infrastructure types out of the domain model.

## Testing Rules
- Test names follow `{Scenario}{ExpectedBehavior}`.
- Focus unit tests on domain behavior and application orchestration.
- Use xUnit and Moq.
- FluentAssertions is banned.

## Delivery Rules
- Show reasoning through concrete architecture outputs, not abstract ceremony.
- Prefer one clear architecture over multiple overlapping patterns.
- Use the latest stable C# supported by the target framework.
Maintain `/.docs/spec/glossary.md` listing domain terms: Term | Definition | Source Authority.

## Global Usings Guidance
Add a project-level `GlobalUsings.cs` (or SDK `ImplicitUsings` + manual additions) to simplify guard invocation:
```csharp
global using static Syrx.Validation.Contract;
```
Do not overuse `using static`; reserve for ubiquitous guard/contract helpers to maintain readability and reduce verbosity. Avoid fully qualified namespace prefixes for guard calls within domain code.

