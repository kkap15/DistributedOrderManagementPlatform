# Microservices Order Processing System

A full-stack microservices application built with Angular, .NET, and Auth0, demonstrating secure communication, API gateway architecture, and resilient service-to-service interactions.

---

## 🚀 Architecture Overview


Angular (Frontend + Auth0)
│
▼
API Gateway (JWT validation, routing)
│
▼
OrderService (business logic)
│
▼
PaymentClient (Polly: retry + circuit breaker)
│
▼
PaymentService (payment processing)

---

## ✅ Features

- 🔐 Auth0 Authentication (login, logout, silent session restore)
- 🌐 Angular Frontend with reactive UI
- 🛡 API Gateway with JWT validation and routing
- 🔄 Request forwarding (headers + body)
- 📦 Order processing microservice
- 💳 Payment service with transaction support
- ♻️ Resilient service communication using Polly:
    - Retry with exponential backoff
    - Circuit breaker pattern
- 🔁 End-to-end REST API integration
- 🔍 Debugging and handling of:
    - CORS issues
    - JSON serialization mismatches
    - Authentication and token propagation
    - API routing errors

---

## 🛠 Tech Stack

- **Frontend**
    - Angular
    - RxJS
    - Auth0 Angular SDK

- **Backend**
    - .NET (ASP.NET Core Web API)
    - API Gateway Pattern
    - HttpClient

- **Resilience**
    - Polly (Retry + Circuit Breaker)

- **Auth**
    - Auth0 (OIDC, JWT)

---

## ⚙️ Setup & Run

### 1. Clone the repository
```bash
git clone <your-repo-url>
cd <your-project>


2. Run services
Start each backend service:
Shellcd Gatewaydotnet runcd OrderServicedotnet runcd PaymentServicedotnet runShow more lines

3. Run Angular frontend
Shellcd frontendng serveShow more lines
Open:
http://localhost:4200


🔑 Auth0 Configuration
Ensure your Auth0 application includes:
Allowed Callback URLs
http://localhost:4200

Allowed Logout URLs
http://localhost:4200

Allowed Web Origins
http://localhost:4200


📌 Key Learnings

Designing and implementing API Gateway architecture
Secure frontend-backend communication using JWT
Handling CORS and preflight requests in ASP.NET Core
Building resilient services using Polly
Debugging distributed system issues (routing, serialization, async)
Managing Angular state and change detection


🚀 Future Improvements

✅ Add database persistence (Orders & Transactions)
✅ Implement audit logging (user-based tracking)
✅ Add monitoring/logging (Serilog / Application Insights)
✅ Introduce containerization (Docker)
✅ Deploy to Azure / cloud platform

👨‍💻 Author
Kanishka Kapoor

🏁 Status
✅ Fully functional end-to-end system
✅ Production-style architecture implemented
