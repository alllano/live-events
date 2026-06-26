# Architecture & Conventions — Backend (.NET)

## 1. Stack and versions
- .NET 10 (LTS)
- ASP.NET Core Web API
- Entity Framework Core (SQL Server, with Migrations)
- FluentValidation
- Swagger / OpenAPI

## 2. Code language

All code is written in **English**: class names, methods, variables, parameters, DTOs, entities, table/column names, constants, internal exception messages, and comments/summary tags. Spanish is reserved exclusively for end-user facing messages (if any) and for context/README documents.

This includes translating the domain concepts from the (Spanish) requirements document when modeling them in code:

| Spanish (requirements) | English (code) |
|---|---|
| Evento | Event |
| Reserva | Reservation / Booking |
| Venue | Venue |
| Entradas | Tickets |
| pendiente_pago | PendingPayment |
| confirmada | Confirmed |
| cancelada | Cancelled |
| perdida (RN-07) | Lost |

## 3. Meaningful names

Beyond casing rules, identifiers must be **self-explanatory**: a reader should understand what a method, variable, or class does/contains without needing to read its implementation or an additional comment.

Practical rules:
- Avoid ambiguous abbreviations (`qty` → `quantity`, `evt` → `event`).
- Boolean-returning methods read as a question (`IsOverlapping`, `HasAvailableCapacity`, `CanBeCancelled`).
- Action methods start with a clear verb (`CreateEvent`, `ConfirmPayment`, `CancelReservation`), not a bare noun.
- Boolean variables follow the same principle as methods (`isActive`, `hasPenalty`) — never ambiguous names like `flag` or `check`.
- Avoid generic names (`data`, `obj`, `temp`, `result`) when a more specific domain name is available (`createdEvent`, `existingReservation`).

## 4. Solution structure

Each layer is an **independent project** within the solution (not folders inside a single project):

- **App.Common**: DTOs, constants, enums. No business logic, no dependency on any other layer.
- **App.Domain**: Services and Service interfaces. Contains business logic and business rules (BR-01 to BR-07). Does not contain entities or direct data access.
- **App.Infrastructure**: Entities, DbContext, Repositories and Repository interfaces, EF Core configuration (Migrations, table mapping via Fluent API), Validators (FluentValidation), AutoMapper profiles (Entity ↔ DTO).
- **App.web**: Controllers, API configuration (Program.cs, dependency injection, middleware, Swagger).
- *App.Config: omitted — not justified by the scope of this test.*

**Dependency direction:**
```
App.web → App.Domain → App.Infrastructure → (DbContext)
   ↓            ↓              ↓
        App.Common (consumed by all layers, depends on none)
```

**Why AutoMapper profiles live in App.Infrastructure, not App.Common**: a mapping profile needs visibility of both sides of the mapping (Entity and DTO). Since App.Common must remain free of any dependency on other layers, the profile lives in Infrastructure, which already references the Entities and can also reference Common's DTOs.

**Dependency injection pattern**: interface and implementation in separate files for every Service and Repository. Interfaces expose only the public methods needed from other layers.

**DI lifetime**: all Services and Repositories are registered as `Scoped`.

## 5. Persistence

- SQL Server with EF Core and **Migrations**.
- `Database.Migrate()` runs on application startup (no manual `dotnet ef database update` step required).
- The 3 reference venues are seeded on startup.
- Connection string lives in `appsettings.json` with placeholder keys (visible, no real values); real values live in a local, non-versioned file (`.gitignore`) and, in production, in Azure App Service Application Settings.
- **Production database**: Azure SQL Database, free tier (General Purpose, serverless — 100,000 vCore-seconds and 32 GB included per month, at no cost). Note: under the serverless auto-pause behavior, the first request after a period of inactivity may take 1-2 minutes to respond while the database resumes — this is expected free-tier behavior, not an application failure.

## 6. Error and exception handling

**Global middleware** catches all unhandled exceptions — no repeated try/catch inside controllers.

**Business exception hierarchy:**
```
AppException (abstract)
├── BusinessRuleException            → 422 Unprocessable Entity
├── NotFoundException                → 404 Not Found
├── InvalidStateTransitionException  → 409 Conflict
└── ValidationException              → 400 Bad Request
```

An error log table in the database stores **all** exceptions caught by the middleware (message, stack trace, endpoint, timestamp), distinguished by `LogTypeId`:

- `LogTypeIds.HandledError`: any exception from the `AppException` hierarchy — expected business rejections, already mapped to a specific status code. Logged for full traceability (e.g., how often a given business rule is being rejected), without exposing internal details to the client.
- `LogTypeIds.UnhandledError`: any exception not part of the `AppException` hierarchy — genuinely unexpected failures. This is the subset worth actively monitoring; filtering by this type alone surfaces real bugs without the noise of routine business-rule rejections.

The client-facing response only ever includes a safe message (`ex.Message` for `AppException`, a generic message for unhandled exceptions) — stack traces are never returned to the client regardless of log type.

## 7. Validation

Two layers, separated by responsibility:

- **FluentValidation** (App.Infrastructure/Validators, inheriting from `AbstractValidator<T>`): DTO shape validation that doesn't require querying data — length, format, required fields, ranges (e.g., title 5-100 chars, valid email, quantity ≥ 1).
- **Services (App.Domain)**: business rules requiring repository access or cross-data checks — venue capacity, schedule overlap, date/time restrictions, transaction limits (BR-01 to BR-07). These throw exceptions from the hierarchy above.

## 8. API response format

A standard response wrapper (`ResponseDTO` / `AsResponseDTO<T>`) is used consistently across all endpoints, including errors caught by the middleware. Controllers do not implement their own try/catch.

## 9. Naming conventions

| Element | Convention |
|---|---|
| Files, classes, methods | PascalCase |
| Contracts, DTOs, Entities (names and fields) | PascalCase |
| Variables | camelCase |
| Constants (in constants files) | UPPER_SNAKE_CASE *(differs from Microsoft's guidance, which uses PascalCase; adopted for consistency with team convention)* |

**Explicit typing**: `var` is not used. Every variable is declared with its corresponding type (`string`, `int`, `DateTime`, `List<T>`, DTOs, etc.), except where the type is genuinely impractical to express explicitly.

## 10. Documentation (XML summary)

- **Required**: public Controller methods and Service interface methods.
- **Not documented**: Repository methods or private methods — their signature is already self-explanatory (basic CRUD operations).

## 11. REST API

- **URL versioning**: `api/v1/[controller]`, without a versioning library given the scope.
- Resources as nouns, not actions. GET for queries/listing (with query-param filters), POST for creation, PATCH for state transitions on an existing resource.
  - Example: `PATCH /api/v1/reservations/{id}/confirm`, `PATCH /api/v1/reservations/{id}/cancel` — instead of a generic PUT with the new state in the body.
- Swagger enabled for interactive documentation.

## 12. CORS

Allowed origins are configurable via `appsettings.json` (`CorsSettings:AllowedOrigins`), not hardcoded in `Program.cs`. Allows adding the production URL (Azure Static Web App) without touching code.

## 13. Deployment

- Backend: Azure App Service (free F1 tier).
- Frontend: Azure Static Web Apps.
- Database: Azure SQL Database (free tier).
- Continuous deployment directly from GitHub (no Azure DevOps — that pattern applies to microfrontends using Module/Native Federation, which is not the case for this test).
- Production secrets via Azure App Service Application Settings — not versioned in the repository. Azure Key Vault was considered and discarded as not justified for this scope.

## 14. Scope and exclusions

- No authentication/authorization implemented — not requested in the functional requirements.
- No Venue management implemented — defined as pre-existing reference data per the requirements.