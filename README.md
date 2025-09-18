
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
- CI (GitHub Actions) with build + test + migration script validation
- Rate limiting / throttling
- Caching (e.g. MemoryCache or Redis) for heavy list endpoints

## License
Internal/private. Adapt as needed before publishing.

---
Questions or suggestions? Open an issue or create a PR.
