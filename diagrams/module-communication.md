# Module communication flows

The solution is a modular monolith with two modules: **Pricing** (Topic 1, discount engine) and **Customers** (Topic 2, dynamic fields). Each module owns its SQL Server schema and its own layers. The modules never reference each other; architecture tests in `test/SubscriptionCommerce.ArchitectureTests` fail the build if they do.

## 1. Module overview

```mermaid
flowchart LR
    Client(["API client"])

    subgraph Host["SubscriptionCommerce.Api (composition root)"]
        MW["Serilog request logging,<br/>OpenTelemetry, Problem Details"]
    end

    subgraph Pricing["Pricing module (Topic 1)"]
        PP["Presentation<br/>POST /orders/discounts"]
        PA["Application<br/>CalculateOrderDiscounts handler<br/>+ discount providers"]
        PD["Domain<br/>Discount, DiscountBreakdown"]
        PI["Infrastructure<br/>price list / promotion / coupon readers<br/>(EF Core)"]
    end

    subgraph Customers["Customers module (Topic 2)"]
        CP["Presentation<br/>tenant, customer, field and value endpoints"]
        CA["Application<br/>commands + queries"]
        CD["Domain<br/>CustomFieldDefinition,<br/>CustomerFieldValue"]
        CI["Infrastructure<br/>repositories, EF Core, rowversion"]
    end

    Common["Common<br/>Result/Error, Money, handler decorators,<br/>IEndpoint, ICurrentUser"]
    CC["Customers.Contracts<br/>(not built: how this would grow)"]
    DB[("SQL Server<br/>schemas: pricing, customers")]

    Client --> Host
    Host --> PP & CP
    PP --> PA --> PD
    PI -. implements readers .-> PA
    PI --> DB
    CP --> CA --> CD
    CI -. implements repositories .-> CD
    CI --> DB
    Pricing & Customers --> Common
    PA -. "only through a<br/>public contract" .-> CC
```

Requests enter through the API host, which maps each module's endpoints. Inside a module, dependencies point inward: endpoints call application handlers, handlers use the domain, and infrastructure implements the data access defined further in. The only allowed path between modules would be a public contract project such as `Customers.Contracts`. The brief doesn't need the modules to talk, so that project does not exist; flow 4 shows how it would be added.

## 2. Calculate discounts (inside Pricing)

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant EP as Pricing endpoint
    participant H as CalculateOrderDiscounts<br/>handler + decorators
    participant PL as Price list provider
    participant PR as Promotion provider
    participant CP as Coupon provider
    participant DB as SQL Server (pricing)
    participant D as DiscountBreakdown<br/>(domain)

    Client->>EP: POST /orders/discounts (customer, price, coupon codes)
    EP->>H: Handle(query)
    Note over H: Exception, logging and validation decorators run first

    H->>PL: GetEligibleDiscountsAsync(order), priority 100
    PL->>DB: price list discounts of the customer
    DB-->>PL: Gold price list 5%
    PL-->>H: [5%]

    H->>PR: GetEligibleDiscountsAsync(order), priority 200
    PR->>DB: promotions
    DB-->>PR: Autumn promotion 10% (active, inside its window)
    PR-->>H: [10%]

    H->>CP: GetEligibleDiscountsAsync(order), priority 300
    CP->>DB: coupons with the order's codes
    DB-->>CP: WELCOME10, -10 EUR
    CP-->>H: [-10 EUR]

    H->>D: Apply(340 EUR, discounts in priority order)
    D-->>H: -17.00, -32.30, -10.00, final 280.70

    alt every provider succeeded
        H-->>EP: Result.Success(breakdown)
        EP-->>Client: 200 OK with the breakdown
    else a provider failed
        H-->>EP: Result.Failure(error), or the exception propagates
        EP-->>Client: Problem Details, never a partial price
    end
```

Providers run one after another in priority order. Each provider owns its eligibility rules; the readers only fetch rows. The domain `DiscountBreakdown` applies the discounts to the running price without any I/O, so the arithmetic is fully unit-tested.

## 3. Set a customer field value (inside Customers)

```mermaid
sequenceDiagram
    autonumber
    actor User as Logged-in user
    participant EP as Customers endpoint
    participant H as SetCustomerFieldValue<br/>handler + decorators
    participant R as Repositories<br/>(EF Core)
    participant A as CustomerFieldValue<br/>(aggregate)
    participant DB as SQL Server (customers)

    User->>EP: PUT /customers/{id}/fields/{fieldId}/value (value, rowVersion)
    EP->>H: Handle(command)
    H->>R: load customer, field with its options, current value
    R->>DB: SELECT
    DB-->>R: rows

    alt rowVersion is not the stored one
        H-->>EP: Result.Failure(Conflict)
        EP-->>User: 409 Problem Details, reload and retry
    else rowVersion matches
        H->>A: Change(field, newValue, changedBy, utcNow)
        alt same value as before
            A-->>H: false: nothing changed, no history entry
            H-->>EP: Result.Success(changed = false)
            EP-->>User: 200 OK
        else value changed
            A->>A: validate, update the value, append a history entry
            H->>R: SaveChangesAsync (one transaction)
            R->>DB: UPDATE value WHERE RowVersion = loaded one, INSERT history row
            alt nobody saved in between
                DB-->>R: 1 row updated
                H-->>EP: Result.Success(new rowVersion)
                EP-->>User: 200 OK
            else someone saved in between
                DB-->>R: 0 rows updated, nothing committed
                H-->>EP: Result.Failure(Conflict)
                EP-->>User: 409 Problem Details
            end
        end
    end
```

The aggregate decides whether anything changed. Only a real change produces a history entry, and the value and its history row commit in the same `SaveChanges`. The row version is checked twice: against the caller's copy before anything is written, and by SQL Server in the `UPDATE`, which catches a writer that saved between our read and our write. A first value has no row version yet; two simultaneous first values meet the unique index on `(CustomerId, FieldDefinitionId)` and the loser also gets 409.

## 4. Cross-module: how this would grow

Example rule: an "Education discount" promotion applies only to customers whose custom field `Industry` is `Education`. Pricing then needs data owned by Customers. There are two ways to get it.

### 4a. Synchronous, through a public contract (recommended inside one process)

```mermaid
sequenceDiagram
    autonumber
    participant PR as Promotion provider<br/>(Pricing)
    participant API as ICustomerProfileApi<br/>(Customers.Contracts)
    participant Q as GetCustomerFieldValues<br/>query (Customers)
    participant DB as SQL Server (customers)

    Note over PR: Promotion "Education discount" requires Industry = Education
    PR->>API: GetFieldValuesAsync(customerId, ["Industry"])
    API->>Q: in-process call, no HTTP
    Q->>DB: SELECT current values (EF Core, AsNoTracking)
    DB-->>Q: Industry = Education
    Q-->>API: CustomerProfileSnapshot
    API-->>PR: CustomerProfileSnapshot
    PR->>PR: promotion is eligible, return the discount
```

Always up to date and simple. Pricing would reference only `Customers.Contracts`, and the architecture test would allow exactly that project. The cost is that Pricing depends on Customers at calculation time, which is acceptable in one process and one database.

### 4b. Asynchronous, through an integration event (if Pricing were deployed separately)

```mermaid
sequenceDiagram
    autonumber
    participant H as SetCustomerFieldValue<br/>handler (Customers)
    participant DB as SQL Server<br/>(value, history, outbox)
    participant RL as Outbox relay
    participant BUS as Message bus
    participant EH as Integration-event handler<br/>(Pricing)
    participant RM as Pricing read model

    H->>DB: one transaction: value + history row + outbox message
    Note over H,DB: History never goes through the bus. It commits with the value
    RL->>DB: poll unpublished outbox messages
    DB-->>RL: CustomerFieldValueChanged
    RL->>BUS: publish CustomerFieldValueChanged
    BUS->>EH: deliver (at least once)
    EH->>EH: skip if the event id was already processed (inbox)
    EH->>RM: upsert the local copy of the customer's Industry
    Note over EH,RM: The promotion provider reads its local copy, eventually consistent
```

Pricing keeps its own copy and keeps working when Customers is down, but the copy is eventually consistent and there are more moving parts. This is the outbox/inbox setup the evently reference uses; it was left out here because nothing needs it yet.

| | 4a. Sync public contract | 4b. Async integration event |
|---|---|---|
| Freshness | Always current | Eventually consistent |
| Coupling | Pricing needs Customers at runtime | Pricing works if Customers is down |
| Moving parts | One interface | Outbox, relay, bus, inbox, read model |
| When to choose | Modules share a process and database | Modules are deployed separately |
