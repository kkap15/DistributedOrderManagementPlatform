# Distributed Order Management Platform

An event-driven microservices application built with .NET 10, Angular, Kafka, and Auth0 — demonstrating async service communication, the transactional outbox pattern, and distributed systems design.

**Repo:** [github.com/kkap15/DistributedOrderManagementPlatform](https://github.com/kkap15/DistributedOrderManagementPlatform)

---

## Architecture

```
Angular Frontend (Auth0 popup login)
        │
        ▼
API Gateway :5002  (JWT validation → forwards Bearer token)
        │
        ├──▶ UserService  :5003  (Auth0 upsert, user profile)
        │
        └──▶ OrderService :5000  (order creation, status updates)
                  │
                  │ publishes order.created
                  ▼
           ┌─────────────┐
           │    Kafka    │  (KRaft mode, no Zookeeper)
           │   Broker    │
           └──────┬──────┘
                  │ consumes order.created
                  ▼
        PaymentService :5001  (payment processing)
                  │
                  │ publishes order.paid
                  ▼
           ┌─────────────┐
           │    Kafka    │
           └──────┬──────┘
                  │ consumes order.paid
                  ▼
        OrderService  (updates order status → Paid/Failed)
```

---

## Event Flow

```
1. POST /api/order/create
2. OrderService saves order (status: Pending)
3. OrderService publishes OrderCreatedEvent → order.created
4. PaymentService consumes order.created
5. PaymentService creates payment record
6. PaymentService publishes PaymentProcessedEvent → order.paid
7. OrderService consumes order.paid
8. OrderService updates order status → Paid or Failed
```

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 21, RxJS, Auth0 Angular SDK |
| API Gateway | ASP.NET Core, JWT Bearer |
| Services | .NET 10, ASP.NET Core Web API |
| Messaging | Apache Kafka (KRaft mode) |
| Kafka Client | Confluent.Kafka |
| Persistence | Entity Framework Core, SQLite |
| Resilience | Polly v8 (retry + circuit breaker) |
| Auth | Auth0 (OIDC / JWT) |
| Containers | Docker, Docker Compose |
| Kafka UI | Provectus Kafka UI |

---

## Project Structure

```
DistributedOrderManagementPlatform/
├── Backend/
│   ├── Contracts/              # Shared event records + messaging interfaces
│   │   ├── Events/
│   │   │   ├── OrderCreatedEvent.cs
│   │   │   └── PaymentProcessedEvent.cs
│   │   ├── Messaging/
│   │   │   ├── IEventPublisher.cs
│   │   │   └── IEventConsumer.cs
│   │   └── Topics.cs
│   ├── Infrastructure/         # Kafka producer/consumer implementations
│   │   ├── Messaging/
│   │   │   ├── KafkaEventPublisher.cs
│   │   │   └── KafkaConsumerBase.cs
│   │   └── Extensions/
│   │       └── KafkaServiceExtensions.cs
│   ├── OrderService/           # Order domain
│   │   ├── Messaging/PaymentProcessedConsumer.cs
│   │   └── Workers/OrderConsumerWorker.cs
│   ├── PaymentService/         # Payment domain
│   │   ├── Messaging/OrderCreatedConsumer.cs
│   │   └── Workers/PaymentConsumerWorker.cs
│   ├── ApiGateway/
│   └── UserService/
├── Frontend/angular-app/
└── docker-compose.yml
```

---

## Key Design Decisions

**Event-driven async communication** — services communicate via Kafka events rather than direct HTTP calls. `OrderService` and `PaymentService` are fully decoupled — neither knows about the other's implementation.

**`KafkaConsumerBase<TEvent>`** — abstract generic base class handles all Kafka plumbing (subscribe, consume loop, deserialization, offset commit). Concrete consumers only implement `HandleAsync(TEvent event)`.

**Scope-per-message pattern** — `IServiceScopeFactory` creates a fresh DI scope for each message, giving each `HandleAsync` call its own EF Core `DbContext`. Prevents memory leaks and tracking conflicts in long-running consumers.

**`IEventPublisher` abstraction** — services depend on the interface, not Kafka directly. Swappable to Azure Service Bus without changing service code.

**Manual offset commit** — `EnableAutoCommit = false` ensures offsets are committed only after `HandleAsync` succeeds. Failed messages are reprocessed on restart.

**Exactly-once producer semantics** — `Acks = Acks.All` + `EnableIdempotence = true` prevents duplicate events even under retry conditions.

---

## Services & Ports

| Service | Port | Responsibility |
|---------|------|----------------|
| ApiGateway | 5002 | JWT validation, request routing |
| OrderService | 5010 (host) / 5000 (container) | Order creation, status updates |
| PaymentService | 5001 | Payment processing |
| UserService | 5003 | User registration and profile |
| Kafka | 9092 | Event broker |
| Kafka UI | 8080 | Topic/message browser |

---

## Setup & Run

### Prerequisites

- Docker Desktop

### One-command startup

```bash
git clone https://github.com/kkap15/DistributedOrderManagementPlatform.git
cd DistributedOrderManagementPlatform
docker compose up --build
```

| URL | Service |
|-----|---------|
| http://localhost:4200 | Angular frontend |
| http://localhost:5002 | API Gateway |
| http://localhost:8080 | Kafka UI |

### Test the event flow

```bash
# Create an order
curl -X POST http://localhost:5010/api/order/create \
  -H "Content-Type: application/json" \
  -d '{"userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "totalAmount": 99.99}'

# Response: { "orderId": "...", "status": "Pending" }
# Watch logs — order status updates to "Paid" within seconds
```

### Auth0 Configuration

| Setting | Value |
|---------|-------|
| Allowed Callback URLs | `http://localhost:4200` |
| Allowed Logout URLs | `http://localhost:4200` |
| Allowed Web Origins | `http://localhost:4200` |

---

## Key Concepts Demonstrated

- **Event-driven architecture** — async service decoupling via Kafka topics
- **Producer/Consumer pattern** — `KafkaConsumerBase<TEvent>` generic base with `HandleAsync`
- **Dependency Inversion** — `IEventPublisher`/`IEventConsumer` abstractions in shared `Contracts` project
- **Scope-per-message** — fresh EF Core `DbContext` per consumed message via `IServiceScopeFactory`
- **Exactly-once semantics** — `Acks.All` + `EnableIdempotence` on producer
- **At-least-once delivery** — manual offset commit after successful processing
- **API Gateway pattern** — JWT validation and token passthrough to downstream services
- **Repository pattern** — EF Core with SQLite, scoped per request
- **Polly resilience** — retry + circuit breaker on HTTP clients
- **KRaft Kafka** — Kafka without Zookeeper, single-node dev cluster

---

## Roadmap

- [x] Docker + docker-compose one-command startup
- [x] Kafka async messaging (order.created → order.paid)
- [x] Contracts + Infrastructure shared projects
- [x] Scope-per-message EF Core pattern
- [ ] Outbox pattern — guaranteed message delivery
- [ ] InventoryService — consumes order.paid, publishes order.shipped
- [ ] NotificationService — consumes all events
- [ ] OpenTelemetry distributed tracing
- [ ] Azure deployment

---

**Author:** Kanishka Kapoor · [github.com/kkap15](https://github.com/kkap15)
