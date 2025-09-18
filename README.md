
# LMS Backend API

Ett modernt .NET 9-baserat REST API för Learning Management System (LMS).

**Frontendutvecklare? Läs API-guiden här:**
[API-FRONTEND-GUIDE.md](API-FRONTEND-GUIDE.md)

## Kom igång
1. Klona repot:
   ```sh
   git clone <repo-url>
   cd LMS-backend-privat
   ```
2. Bygg och starta:
   ```sh
   dotnet build LMS.API
   dotnet run --project LMS.API
   ```
3. Öppna Swagger:
   [https://localhost:7213/swagger](https://localhost:7213/swagger)

### Testanvändare
| Roll     | E-post             | Lösenord                |
|----------|--------------------|-------------------------|
| Admin    | admin@test.com     | ChangeThisDevOnly123!   |
| Teacher  | teacher@test.com   | password                |
| Student  | student@test.com   | password                |
Fler testanvändare skapas automatiskt vid första start.

## Arkitektur
Projektet är uppdelat i lager:
- API (controllers, swagger, auth)
- Presentation
- Application (affärslogik)
- Application.Contracts (DTOs, kontrakt)
- Domain (modeller)
- Infrastructure (databas, migrationer)
- Shared (hjälpklasser)

## API-resurser
Exempel på endpoints:
- /api/auth/login (inloggning, JWT)
- /api/courses (kurser)
- /api/modules (moduler)
- /api/activities (aktiviteter)
- /api/documents (dokument)

## Vanliga problem
| Problem         | Orsak                | Lösning                       |
|-----------------|----------------------|-------------------------------|
| 401 Unauthorized| Saknar JWT           | Logga in och skicka token     |
| 500 vid seeding | Saknar "password"   | Sätt i user-secrets           |
| DB-låsning      | LocalDB är låst      | Byt DB-namn/stäng anslutning  |

## Tips
- Testa API:et enkelt via Swagger.
- JWT krävs för skyddade endpoints.
- Seeding skapar testdata automatiskt.

---
Frågor eller förslag? Skapa en issue eller PR.

## Komma igång
### 1. Klona repo
```

# LMS Backend API (English Overview)

A modern .NET 9-based REST API for a Learning Management System (LMS). The architecture follows clear layers (API / Presentation / Application / Application.Contracts / Domain / Infrastructure / Shared) and uses Identity for authentication, JWT for access tokens, and Mapster for object mapping.

## Contents
- [Architecture Overview](#architecture-overview)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Authentication & Roles](#authentication--roles)
- [Data Seeding](#data-seeding)
- [API Resources (Overview)](#api-resources-overview)
- [Example Flow](#example-flow)
- [Common Issues & Troubleshooting](#common-issues--troubleshooting)
- [Next Steps / Recommendations](#next-steps--recommendations)

## Architecture Overview
```
LMS.API (Web API, controllers, swagger, auth config)
LMS.Presentation (future presentation-specific concerns)
LMS.Application (business logic / services)
LMS.Application.Contracts (DTOs + repository & service contracts)
LMS.Domain (domain models / entities)
LMS.Infrastructure (EF Core, DbContext, migrations, repository implementations)
LMS.Shared (shared helpers, extensions)
```
Principles:
- Clear separation of concerns
- Dependency Inversion: outer layers reference inward – never the other way
- Idempotent seeding for roles and admin

## Tech Stack
- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 + SQL Server LocalDB
- ASP.NET Core Identity (ApplicationUser)
- JWT (access + refresh token support in service logic)
- Mapster (DTO <-> domain)
- Bogus (data generation for seeding)
- Swagger / OpenAPI (including Bearer auth)

## Getting Started
### 1. Clone the repo
```
git clone <repo-url>
cd LMS-backend-privat
```
### 2. Build the solution
```
dotnet build .\LMS-Backend\LMS.sln
```
### 3. (Optional) Add secrets
```
cd .\LMS-Backend\LMS.API
dotnet user-secrets init
dotnet user-secrets set "AdminSeed:Password" "StrongerPassword123!"
dotnet user-secrets set "password" "DefaultSeedPassword123!"
```
### 4. Run
```
dotnet run --project .\LMS-Backend\LMS.API\LMS.API.csproj
```

Then open Swagger: `https://localhost:7213/swagger` (or the URL shown in the logs).

## Configuration
`appsettings.json` (excerpt):
```json
{
  "ConnectionStrings": {
    "ApplicationDbContext": "Server=(localdb)\\mssqllocaldb;Database=LmsDB_Fresh;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Issuer": "LmsAPI",
    "Audience": "https://localhost:7213",
    "Expires": 10
  },
  "AdminSeed": {
    "Email": "admin@test.com",
    "UserName": "admin@test.com",
    "Password": "ChangeThisDevOnly123!" // Move to user-secrets in production
  }
}
```
Recommended secrets for user-secrets:
- `AdminSeed:Password`
- `password` (used for demo/test users generated)

## Authentication & Roles
- Registered via Identity
- JWT generated on login (AuthController – see Swagger for full docs)
- Roles: `Admin`, `Teacher`, `Student`
- Admin is always seeded if missing (config via `AdminSeed`)

Send token in requests:
```
Authorization: Bearer <your_access_token>
```

## Data Seeding
Seeding runs only in Development environment via the hosted service `DataSeedHostingService`.
1. Roles ensured (Admin/Teacher/Student)
2. Admin created (idempotent)
3. If database is empty:
   - Demo teacher + student accounts
   - 20 random users
   - Example courses (3), modules (2 per course), activities & documents

Change seeding behavior: Edit the class `DataSeedHostingService` in `LMS.API/Services`.

## API Resources (Overview)
(Simplified – check controllers in `LMS.API/Controllers` for exact endpoints.)
- Auth: Login, Refresh (JWT)
- Courses: CRUD, link to Teacher + Students, modules & documents
- Modules: CRUD (per course)
- Activities: CRUD (per module)
- Documents: CRUD (linked to Course/Module/Activity by type + id)

DTOs are in `LMS.Application.Contracts/DTOs`.

## Example Flow
1. Start API -> seeding runs
2. Log in as admin (`admin@test.com` / configured password) -> get JWT
3. Create new course (POST /api/courses)
4. Add module (POST /api/courses/{courseId}/modules)
5. Add activity (POST /api/modules/{moduleId}/activities)
6. Upload (create) document (POST /api/documents)

## Common Issues & Troubleshooting
| Problem | Cause | Solution |
|---------|-------|----------|
| 401 Unauthorized | Missing or invalid JWT | Log in and send Bearer token |
| 500 on seeding | Missing `password` in secrets | Set `password` in user-secrets |
| Database error on migration | Locked LocalDB | Change DB name or close connections |
| Roles missing | Seeding disabled (not Development) | Check `ASPNETCORE_ENVIRONMENT` |

## Next Steps / Recommendations
- Add unit tests (e.g. xUnit) for application services
- Add role-based policies: `[Authorize(Policy = "RequireAdmin")]`
- Health checks (`/health` endpoint)
- Serilog + structured logging
- CI (GitHub Actions) with build + test + migration script validation
- Rate limiting / throttling
- Caching (e.g. MemoryCache or Redis) for heavy list endpoints

## License
Internal/private. Adapt as needed before publishing.

---
Questions or suggestions? Open an issue or create a PR.
