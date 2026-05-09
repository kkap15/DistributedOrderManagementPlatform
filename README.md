# Distributed Order Management Platform

A full-stack microservices application built with Angular, .NET 10, and Auth0 — demonstrating secure user authentication, API gateway routing, database persistence, and resilient service-to-service communication.

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
        └──▶ OrderService :5000  (order management, per-user filtering)
                  │
                  ▼
        PaymentClient  (Polly: retry + circuit breaker)
                  │
                  ▼
        PaymentService :5001  (payment processing)
```

---

## Features

- **Auth0 Authentication** — popup-based login (no full-page redirect), silent session restore, logout
- **Auto user registration** — first login creates a user record from JWT claims (`sub`, `email`, `name`)
- **Per-user orders** — orders are scoped to the logged-in user via `userId` query filtering
- **API Gateway** — validates JWT, forwards raw Bearer token to downstream services
- **Entity Framework Core + SQLite** — persistent storage for users, orders, and payments with migrations
- **Resilient service communication** — Polly retry (exponential backoff) + circuit breaker on PaymentClient
- **Clean Angular dashboard** — user profile header, orders table, create/refresh actions

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 19, RxJS, Auth0 Angular SDK |
| API Gateway | ASP.NET Core, JWT Bearer |
| Services | .NET 10, ASP.NET Core Web API |
| Persistence | Entity Framework Core, SQLite |
| Resilience | Polly v8 (ResiliencePipelineBuilder) |
| Auth | Auth0 (OIDC / JWT) |

---

## Services & Ports

| Service | Port | Responsibility |
|---|---|---|
| ApiGateway | 5002 | JWT validation, request routing |
| OrderService | 5000 | Order creation, per-user order retrieval |
| PaymentService | 5001 | Payment processing |
| UserService | 5003 | User registration and profile |

---

## Setup & Run

### 1. Clone
```bash
git clone <your-repo-url>
cd DistributedOrderManagementPlatform
```

### 2. Run backend services (each in a separate terminal)
```bash
cd Backend/ApiGateway      && dotnet run
cd Backend/OrderService    && dotnet run
cd Backend/PaymentService  && dotnet run
cd Backend/UserService     && dotnet run
```

EF Core migrations run automatically on startup — SQLite databases are created in each service directory.

### 3. Run Angular frontend
```bash
cd Frontend/angular-app
npm install
ng serve
```

Open [http://localhost:4200](http://localhost:4200)

---

## Auth0 Configuration

In your Auth0 Application settings:

| Setting | Value |
|---|---|
| Allowed Callback URLs | `http://localhost:4200` |
| Allowed Logout URLs | `http://localhost:4200` |
| Allowed Web Origins | `http://localhost:4200` |

---

## Key Concepts Demonstrated

- API Gateway pattern with JWT passthrough to downstream services
- First-login user upsert using Auth0 JWT claims (`MapInboundClaims = false`)
- Polly v8 `ResiliencePipelineBuilder` with retry and circuit breaker
- Repository pattern with EF Core and SQLite
- Angular standalone components with Auth0 popup login flow
- Handling distributed system issues: routing mismatches, circuit breaker state, claim mapping

---

## Roadmap

- [ ] Docker + docker-compose for one-command startup
- [ ] Serilog structured logging across all services
- [ ] Azure deployment
- [ ] gRPC or message bus (RabbitMQ) for async service communication

---

**Author:** Kanishka Kapoor
