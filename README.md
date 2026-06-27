# EventosVivos

Sistema de reservas de entradas para eventos culturales, conferencias y talleres. Resuelve el control de inventario en tiempo real, la gestión de disponibilidad de venues compartidos entre eventos, y el ciclo de vida completo de una reserva (pago → confirmación → cancelación, con políticas de penalización por cancelaciones tardías).

## URLs desplegadas

- **Frontend**: https://blue-meadow-09d38a410.7.azurestaticapps.net
- **Backend (Swagger)**: https://live-events-api.azurewebsites.net/swagger

> **Nota sobre Swagger en producción**: Swagger está expuesto deliberadamente en el ambiente de producción para esta prueba técnica, de forma que el evaluador pueda explorar y probar la API en vivo sin necesidad de herramientas adicionales. En un sistema productivo real, esto se restringiría a ambientes no productivos (desarrollo/staging).

## Stack tecnológico

- **Backend**: .NET 10, ASP.NET Core Web API, Entity Framework Core, FluentValidation, AutoMapper, xUnit + Moq.
- **Base de datos**: SQL Server vía Azure SQL Database (tier gratuito).
- **Frontend**: Angular 21.2, Signal Forms (`@angular/forms/signals`), Vitest. *(El enunciado pedía Angular 22/última versión; 21.2 fue, al momento del desarrollo, la versión estable más reciente disponible.)*

## Arquitectura

El backend está separado en 4 proyectos independientes (`App.Common`, `App.Domain`, `App.Infrastructure`, `App.web`) con una dirección de dependencias explícita, evitando que la lógica de negocio dependa de detalles de persistencia. El frontend separa vistas `external` (portal público) de `internal` (panel administrativo), con un `shared` para lo que usan ambos. La disponibilidad de entradas se calcula dinámicamente a partir de las reservas existentes en cada consulta, en lugar de mantener contadores persistidos en el evento, evitando que un contador quede desincronizado por una actualización fallida.

Para el detalle completo de cada decisión:
- [backend/ARCHITECTURE.md](backend/ARCHITECTURE.md)
- [frontend/ARCHITECTURE.md](frontend/ARCHITECTURE.md)
- [backend/DATA-MODEL.md](backend/DATA-MODEL.md)

## Cómo ejecutar localmente

### Backend

**Prerrequisitos**: .NET 10 SDK, SQL Server (local o una instancia de Azure SQL).

1. En `backend/App.web/appsettings.Development.json` (no versionado), configura el connection string real:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "<tu-connection-string>"
     }
   }
   ```
2. Desde `backend/App.web`, ejecuta:
   ```
   dotnet run
   ```
   Las migrations de EF Core se aplican automáticamente al arrancar (`Database.Migrate()`) — no se requiere ningún paso manual de `dotnet ef database update`.

### Frontend

**Prerrequisitos**: Node.js, Angular CLI.

1. Desde `frontend/app`, instala dependencias:
   ```
   npm install
   ```
2. Verifica que `src/environments/environment.ts` apunte a tu backend local (por defecto, `https://localhost:7073/api/v1`).
3. Levanta la app:
   ```
   ng serve
   ```

### Tests

- **Backend**: desde `backend/`, ejecuta `dotnet test`.
- **Frontend**: desde `frontend/app`, ejecuta `ng test`.

## Despliegue

El proyecto se despliega vía CI/CD con GitHub Actions: cada push a `main` dispara automáticamente el build y despliegue tanto del backend como del frontend, sin pasos manuales. Infraestructura en Azure, toda en tier gratuito:

- **Backend**: Azure App Service.
- **Frontend**: Azure Static Web Apps.
- **Base de datos**: Azure SQL Database.

## Alcance y exclusiones

Las siguientes exclusiones son decisiones deliberadas, no omisiones:

- **Sin autenticación/autorización**: no fue solicitada en los requerimientos funcionales.
- **Sin gestión de Venues (CRUD)**: los venues son datos de referencia preexistentes, según el enunciado.
- **La liberación de tickets "perdidos" (estado `Lost`, BR-07) no es automática**: cuando una reserva confirmada se cancela con menos de 48 horas de anticipación al evento, sus tickets quedan marcados como perdidos y no se liberan para venta. Devolverlos a disponibilidad requiere una acción manual del administrador vía `PATCH /reservations/{id}/release`.

## Estructura del repositorio

```
.
├── backend/              # Solución .NET (4 proyectos + tests)
│   ├── ARCHITECTURE.md     # Decisiones de arquitectura del backend
│   └── DATA-MODEL.md       # Modelo de datos y su razonamiento
├── frontend/
│   ├── app/                # Aplicación Angular
│   └── ARCHITECTURE.md     # Decisiones de arquitectura del frontend
├── CONTEXT.md             # Enunciado y reglas de negocio de la prueba técnica
└── README.md              # Este archivo
```

`CONTEXT.md`, los `ARCHITECTURE.md` y `DATA-MODEL.md` documentan en detalle el razonamiento técnico detrás de cada decisión, para quien quiera profundizar más allá de este resumen.
