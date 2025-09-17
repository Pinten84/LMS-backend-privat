# API-guide för frontendutvecklare

Denna guide hjälper dig som frontendutvecklare att förstå och använda LMS Backend API.

## 1. API-dokumentation
- Swagger UI: [https://localhost:7213/swagger](https://localhost:7213/swagger)
- Utforska alla endpoints, se exempel på request/response och testa direkt i webbläsaren.

## 2. Autentisering
- Logga in via `/api/auth/login` med e-post och lösenord.
- Du får en JWT-token i svaret.
- Skicka token i header vid skyddade anrop:
  ```
  Authorization: Bearer <din_token>
  ```

## 3. Roller och behörigheter
- Endpoints kan kräva olika roller:
  - **Admin**: Full tillgång
  - **Teacher**: Skapa/ändra kurser, moduler, aktiviteter, dokument
  - **Student**: Se och delta i kurser

## 4. Testanvändare
| Roll     | E-post             | Lösenord                |
|----------|--------------------|-------------------------|
| Admin    | admin@test.com     | ChangeThisDevOnly123!   |
| Teacher  | teacher@test.com   | DefaultSeedPassword123! |
| Student  | student@test.com   | DefaultSeedPassword123! |

## 5. Exempel på flöde
1. Logga in och hämta JWT-token
2. Skicka token i header för att hämta t.ex. kurser:
   ```
   GET /api/courses
   Authorization: Bearer <din_token>
   ```
3. Skapa, uppdatera eller ta bort resurser via motsvarande endpoints

## 6. Felhantering
| Problem         | Orsak                | Lösning                       |
|-----------------|----------------------|-------------------------------|
| 401 Unauthorized| Saknar JWT           | Logga in och skicka token     |
| 500 vid seeding | Saknar "password"   | Sätt i user-secrets           |
| DB-låsning      | LocalDB är låst      | Byt DB-namn/stäng anslutning  |

## 7. Tips
- Testa API:et via Swagger innan du bygger frontend.
- Seeding skapar testdata automatiskt vid första start.
- Kontakta backend-utvecklare vid frågor om endpoints eller data.

---
