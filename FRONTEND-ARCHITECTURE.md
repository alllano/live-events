# Architecture & Conventions — Frontend (Angular)

## 1. Stack and versions
- Angular 22
- TypeScript
- Signal Forms (`@angular/forms/signals`)
- Vitest (CLI's default test runner)

## 2. Code language

Same principle as the backend: all code is written in **English** — component, service, interface, method, and variable names, constants, routes, and comments/JSDoc. Spanish is reserved for end-user facing text (labels, on-screen validation messages) and for context/README documents.

## 3. Meaningful names

Same rules as the backend, applied to TypeScript:
- Boolean methods read as a question (`isValid`, `hasAvailableTickets`).
- Action methods start with a clear verb (`loadEvents`, `submitReservation`, `cancelReservation`).
- No ambiguous abbreviations or generic names (`evt`, `data`, `temp`) when a clearer domain alternative exists.
- File names describe their content, not a generic label (`event-list.component.ts`, not `list.component.ts`).

## 4. Folder structure

The Angular project lives at `frontend/app/` (generated via `ng new app`), not directly under `frontend/`. All Angular CLI commands (`ng serve`, `ng generate`, `ng test`) must be run from within `frontend/app/`.

```
frontend/app/src/app/
├── views/
│   ├── external/   (event listing, reservation form — public-facing)
│   ├── internal/   (event creation, payment confirmation, occupancy report — admin)
│   └── shared/     (reservation-cancel, used from both external and internal)
├── services/       (event.service.ts, reservation.service.ts, http-base.service.ts)
└── common/         (models, constants, enums)
```

## 5. Naming conventions

| Element | Convention |
|---|---|
| File names | kebab-case + standard Angular suffix (`event.service.ts`, `event-list.component.ts`, `event-detail-response.model.ts` for plain interfaces in `common/models/`) |
| Classes, interfaces (type), components, services | PascalCase |
| Methods | camelCase |
| Model/interface fields | camelCase |
| Variables | camelCase, with explicit type whenever applicable |
| Constants (in constants files) | UPPER_SNAKE_CASE |

*Note: the JSON contract between backend and frontend uses camelCase natively (ASP.NET Core's default serialization), so TypeScript interfaces mirror those exact field names with no additional mapping needed.*

## 6. HTTP and error handling

- A generic base service (`http-base.service.ts`) provides `get<T>()`, `post<T>()`, `put<T>()`, `patch<T>()` methods, returning `Observable<T>`.
- Domain services (`EventService`, `ReservationService`) consume the base service; they do not implement their own error handling.
- HTTP errors are caught centrally through a **global functional interceptor** (`HttpInterceptorFn`).

## 7. Forms

**Signal Forms** (`form()`, schema-based validation with `required`, `validate`, etc.), including cross-field validation for rules such as maximum capacity ≤ venue capacity, or end date after start date.

## 8. State management

Each domain service holds its own `signal()` with loaded data (e.g., the event list), avoiding unnecessary refetching across components.

## 9. Routes

A single `app.routes.ts` file, no lazy loading (not justified by the project's size), split into `external` (user-facing portal) and `internal` (admin panel), with a shared reservation-cancellation component used by both.

## 10. Testing

Vitest, global mode (`globals: true`) — no explicit imports of `describe`/`it`/`expect`.

## 11. Documentation (JSDoc)

- **Required**: public Service methods.
- **Components**: only methods whose logic isn't obvious from their signature.
- **Not documented**: interfaces/models, trivial getters/setters, or the HTTP base service.

## 12. Configuration

`environment.ts` / `environment.prod.ts` hold the API base URL. These are not secrets (the API URL is visible from the browser), so they are versioned in the repository.

## 13. Scope and exclusions

- No authentication implemented.
- No Venue management implemented.