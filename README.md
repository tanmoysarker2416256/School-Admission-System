# School Admission System

Online admission for a school. Each class-and-shift (for example "Class 6 - Morning Shift") has a limited number of seats, handed out first come, first served. Applying holds a seat for a limited time. Paying the fee within that time confirms it, and otherwise the seat is released.

**Stack:** C# / ASP.NET Core (.NET 8), PostgreSQL, React + TypeScript (Vite), Docker.

## Run with Docker

The only requirement is Docker (Docker Desktop on Windows or macOS).

```
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend (open this) | http://localhost:3000 |
| Backend API | http://localhost:8080 |
| PostgreSQL | localhost:5433 (user `postgres`, password `1234`, database `school_admission`; for local use only)|

The database tables are created automatically when the backend starts.

**Stop and restart:** `docker compose down`, then `docker compose up`. The data is kept in a Docker volume (`pgdata`). `docker compose down -v` also deletes the data.

## Configuration (environment variables)

| Variable | Where | Default | Meaning |
|---|---|---|---|
| `ConnectionStrings__Default` | backend (`docker-compose.yml`) | points to the `db` service | Database connection |
| `HOLD_DURATION_SECONDS` | backend (`docker-compose.yml`) | `600` | How long a seat hold lasts |
| `VITE_API_URL` | frontend build argument | `http://localhost:8080` | Backend URL used by the page |

`VITE_API_URL` is baked in when the frontend is built. To change it, set it and rebuild:

```
VITE_API_URL=http://myhost:8080 docker compose up --build
```

## Using the app

1. **Admin** tab: create a class by choosing grade, shift and total seats.
2. **Classes** tab: enter a 17-digit student ID, then press **Apply** on a class.
3. The application panel shows the countdown, **Pay fee** (confirms the seat) and **Cancel application**. When time runs out it shows `EXPIRED` and Pay fee is disabled.
4. **My applications** tab: every application of this student and its status. The ID is remembered in the browser, so a refresh keeps everything.

There is no login. "Pay fee" does not charge anything; it only confirms the seat.

## API examples

Base URL `http://localhost:8080`. Student endpoints need the `X-Student-Id` header (exactly 17 digits). Errors have the form `{ "error": "message" }`.

```
# create a class
curl -X POST http://localhost:8080/classes -H "Content-Type: application/json" -d '{"name":"Class 6 - Morning Shift","totalSeats":120}'

# list classes with seat counts
curl http://localhost:8080/classes

# apply for a seat (201, or 409 when there is no seat or you already have an active application)
curl -X POST http://localhost:8080/classes/{classId}/holds -H "X-Student-Id: 20151234567890123"

# status, confirm, cancel
curl http://localhost:8080/holds/{holdId}
curl -X POST http://localhost:8080/holds/{holdId}/confirm -H "X-Student-Id: 20151234567890123"
curl -X DELETE http://localhost:8080/holds/{holdId} -H "X-Student-Id: 20151234567890123"

# all applications of a student
curl http://localhost:8080/me/holds -H "X-Student-Id: 20151234567890123"
```

In Windows `cmd`, use double quotes and escape the inner ones: `-d "{\"name\":\"Class 6 - Morning Shift\",\"totalSeats\":120}"`.

## Tests

Backend tests (rules and concurrency) run against a real PostgreSQL, because the seat safety depends on database locks. They need the .NET 8 SDK and an empty database whose name ends in `_test`.

With the Docker database running (`docker compose up -d db`):

```
docker compose exec db psql -U postgres -c "CREATE DATABASE school_admission_test;"
```

Then set the connection string and run the tests:

```
# Windows cmd
set TEST_DB_CONNECTION=Host=localhost;Port=5433;Database=school_admission_test;Username=postgres;Password=postgres
dotnet test backend/SchoolAdmission.Tests

# macOS / Linux
export TEST_DB_CONNECTION="Host=localhost;Port=5433;Database=school_admission_test;Username=postgres;Password=postgres"
dotnet test backend/SchoolAdmission.Tests
```

The concurrency tests start many requests at the same moment and check that held + confirmed seats never exceed the total.

## Structure

```
backend/
  SchoolAdmission.Domain/          entities and rules
  SchoolAdmission.Infrastructure/  EF Core, migrations, services with the seat-safety transactions
  SchoolAdmission.Api/             endpoints, startup, background worker
  SchoolAdmission.Tests/           rule and concurrency tests
frontend/                          React + TypeScript (Vite)
docker-compose.yml
```

## How seats stay safe

Applying runs in one database transaction that locks the class row (`SELECT ... FOR UPDATE`), counts held and confirmed seats, and only then inserts the hold. A unique index in PostgreSQL also allows only one active hold or confirmed seat per student. Because the lock lives in the database, it protects the seats even if several backend processes share the same database.

More details and decisions: see [ASSUMPTIONS.md](ASSUMPTIONS.md) and [AI_USAGE.md](AI_USAGE.md).
