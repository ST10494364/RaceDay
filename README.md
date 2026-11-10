# RaceDay

RaceDay is a full-stack, web-based event management system built for the South African road running, walking, and cycling community. It replaces the paper-based registration and disconnected communication that many local events still rely on, giving Event Organisers a single platform to create and manage events, categories, and results, while Participants can browse events, enrol, and track their own results and history.

This project is an individual Portfolio of Evidence (POE), built progressively across three parts:

- **Part 1 — System Planning and Database**: ERD, API endpoint plan, and SQL database schema. *(complete)*
- **Part 2 — RESTful API**: built in C#, connected to the database, with unit tests and CI/CD. *(complete)*
- **Part 3 — MVC Web Application**: consumes the API, integrates Azure Blob Storage, and is containerised with Docker. *(current stage, on the `part-3-mvc` branch)*

## Roles

The system supports two distinct user roles:

- **Organiser** — can create, update, and delete events; define event categories; view all enrolments for their events; confirm enrolments; upload event banner images; and capture participant results.
- **Participant** — can browse and view events and categories, enrol in an event under a chosen category, view their own enrolments, results and profile, and upload a profile picture.

## Repository Structure

/docs → Planning artifacts and screenshots
- FINAL_ERD.drawio.png — Entity Relationship Diagram
- API_Endpoint_Plan.md — Full API endpoint plan
- RaceDay_Schema.sql — SQL script (schema + seed data)
- ci-success.png — Part 1 CI/CD successful build screenshot
- ci-success-part2.png — Part 2 CI/CD successful build screenshot
- part3-ci-green-build.png — Part 3 CI/CD successful build screenshot

 The database design follows a normalised structure: a single `User` table (distinguishing Organisers and Participants via a `role` field) connects to `Event`, which in turn connects to `Category`, `Enrolment`, and `Result` — with `Enrolment` linking Participants, Events, and Categories together. 

/RaceDay.Api → Part 2 ASP.NET Core Web API (controllers, models, DTOs, EF Core migrations), extended in Part 3 with Azure Blob Storage and enrolment status

/RaceDay.Api.Tests → Part 2 xUnit unit tests

/RaceDay.Web → Part 3 ASP.NET Core MVC web application (controllers, views, models, API client), with a Dockerfile

/.github/workflows → CI/CD configuration
- validate-docs.yml — Part 1: validates /docs folder structure
- ci.yml — validates the repository structure, builds the API, runs all unit tests and (Part 3) builds the MVC project

## Part 1 — Setup Instructions

Part 1 contains no application code, so there is nothing to run. To review the planning artifacts:

1. Clone this repository or browse it directly on GitHub.
2. Open `/docs/FINAL_ERD.drawio.png` to view the database design.
3. Open `/docs/API_Endpoint_Plan.md` to view the full endpoint plan (renders as a formatted table on GitHub).
4. To run the SQL script yourself:
   - Open `/docs/RaceDay_Schema.sql` in SQL Server Management Studio (SSMS).
   - Run the full script against a SQL Server instance. It will drop and recreate the `RaceDay` database from scratch, create all 6 tables, and seed sample data.

## Part 1 — CI/CD

A GitHub Actions workflow (`.github/workflows/validate-docs.yml`) runs automatically on every push to `main`. It checks that the `/docs` folder exists and contains all three required planning files.

**Latest build status:**

![CI/CD successful build](docs/ci-success.png)

## Part 1 — Video Walkthrough

An unlisted YouTube video walking through the ERD design decisions, the API endpoint plan, and a live run of the SQL script in SSMS:

https://youtu.be/OaIk2n3y3Wo

## Part 2: RESTful API

The API is built with ASP.NET Core 8 (controllers) and Entity Framework Core 8 (code-first migrations) on SQL Server. It implements the endpoint plan from Part 1, with Swagger documentation, session-based authentication, role-based access control and 43 unit tests.

### Part 2: Setup Instructions

**Requirements:** Visual Studio 2022, .NET 8 SDK, and SQL Server (Express or LocalDB) with SSMS.

1. Clone this repository.
2. Open `RaceDay.Api/RaceDay.Api.sln` in Visual Studio. It contains the API project and the test project.
3. Open `RaceDay.Api/appsettings.json` and change the `Server=` part of the `DefaultConnection` connection string to your own SQL Server instance (for example `localhost\\SQLEXPRESS` or `(localdb)\\MSSQLLocalDB`). The database name is `RaceDayApi` and is created automatically. It is separate from the Part 1 `RaceDay` database.
4. Open **Tools > NuGet Package Manager > Package Manager Console**, check the default project is `RaceDay.Api`, and run: `Update-Database`. This creates the six tables, the constraints and the three seeded event types (Run, Walk, Cycle).
5. Press **F5**. Swagger opens in the browser (`/swagger`).
6. To try the API: register one Organiser and one Participant with `POST /api/auth/register`, log in with `POST /api/auth/login` (this starts a session), then call the other endpoints. Logging in as another user replaces the current session.
7. To run the tests: **Test > Run All Tests** in Visual Studio, or `dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj`.

### Authentication and Roles

- Passwords are hashed with ASP.NET Core's `PasswordHasher` before they are saved. Plain passwords are never stored.
- Login creates a server-side session that stores the user's ID and role (30 minute idle timeout). Every protected endpoint reads the session and returns **401** if nobody is logged in and **403** if the role or ownership is wrong.
- **Organiser:** creates, updates and deletes their own events, defines categories, views enrolments for their own events and captures results.
- **Participant:** enrols in events under a category, and views their own enrolments and results.
- Both roles can view and update their own profile and browse events and categories.

### API Endpoints (16)

| Method | Route | Who can call it |
|---|---|---|
| POST | /api/auth/register | Anyone |
| POST | /api/auth/login | Anyone |
| GET | /api/users/me | Any logged-in user |
| PUT | /api/users/me | Any logged-in user |
| POST | /api/events | Organiser |
| GET | /api/events | Any logged-in user |
| GET | /api/events/{id} | Any logged-in user |
| PUT | /api/events/{id} | Organiser (must own the event) |
| DELETE | /api/events/{id} | Organiser (must own the event) |
| POST | /api/events/{id}/categories | Organiser (must own the event) |
| GET | /api/events/{id}/categories | Any logged-in user |
| POST | /api/events/{id}/enrolments | Participant |
| GET | /api/events/{id}/enrolments | Organiser (must own the event) |
| GET | /api/enrolments/me | Participant |
| POST | /api/events/{id}/results | Organiser (must own the event) |
| GET | /api/results/me | Participant |

### Unit Tests

43 xUnit tests run against an in-memory database. They cover registration and login (including password hashing and wrong credentials), and every other controller, including role and ownership rejection (403), not-logged-in (401), duplicates (409), invalid input (400) and not found (404).

### Part 2 CI/CD

The workflow `.github/workflows/ci.yml` runs on every push to `main`. It validates the repository structure, restores and builds the API, and runs all unit tests.

**Latest build status:**

![Part 2 CI/CD successful build](docs/ci-success-part2.png)

### Part 2 Video Walkthrough

An unlisted YouTube video walking through the API, authentication and roles, Swagger and the unit tests:

VIDEO_LINK_HERE

### Part 2: Deviations from the Part 1 Plan

- **Login:** my Part 1 plan said `/api/auth/login` returns an access token. Part 2 requires session-based authentication, so login now creates a server-side session that stores the user ID and role instead.
- **Added endpoint:** `GET /api/enrolments/me` lets a Participant view their own enrolments. The Roles section of the brief requires this, but it was missing from my Part 1 plan.
- **Endpoint count:** my Part 1 text says 14 endpoints, but the table lists 15. With the added endpoint, the API has 16.
- **Database creation:** the Part 2 database (`RaceDayApi`) is created by EF Core migrations rather than by running the Part 1 SQL script. It has the same six tables, column names, types, keys, unique email, default values and no-cascade foreign keys. The Part 1 sample data is not loaded; only the three event types are seeded.
- **Extra status codes:** the protected endpoints return 401 (not logged in) and 403 (wrong role or not the event's owner), and the create and update endpoints return 400 for invalid input, in addition to the codes in my Part 1 plan.
- **Deleting an event** also deletes its categories, enrolments and results, because the database does not cascade deletes.

## Part 3: RaceDay Web (ASP.NET Core MVC), Azure Blob Storage and Docker

Part 3 adds the web front end for RaceDay. The MVC app (`RaceDay.Web`) has no database access and no business logic of its own: every action goes through the Part 2 API (`RaceDay.Api`). Work for this part is on the `part-3-mvc` branch.

### The two roles in the web app

**Organiser**
- Dashboard with events created, upcoming events and enrolments per event
- Create, edit and delete events, with an event banner image upload
- Add categories to an event
- View all enrolments for an event and confirm Pending enrolments
- Capture finish times and finishing positions after an event

**Participant**
- Browse upcoming events with search and event-type filters
- Event details page with categories, and enter an event under a chosen category
- My Events (with colour-coded enrolment status: amber Pending, green Confirmed)
- My Results (finish time and position, for example "47th out of 312 finishers")
- Profile page to update personal information and upload a profile picture

### How it fits together

- **MVC to API:** `RaceDay.Web/Services/ApiClient.cs` is the only class that talks to the API. After login it keeps the API's session cookie inside the MVC session and sends it with every call.
- **Session and navigation:** the MVC app stores the user's id, role and name in session. Protected pages redirect visitors to Login, each role has its own menu, and logging out clears the session.
- **Azure Blob Storage:** images are uploaded from the MVC app to the API. The API stores them in a private Azure container (`raceday-images`) and saves the blob URL in the database (`banner_image_url` on Event, `profile_picture_url` on User). The browser never talks to Azure: the API (through the MVC app) streams the images back.
- **Secrets:** the Azure connection string is kept in the API's user secrets and is never committed to GitHub.
- **UI:** Bootstrap with a custom navy and orange RaceDay theme, a role-specific menu, colour-coded statuses and friendly error messages instead of raw errors.

### Changes made to the API in Part 3

- **Azure Blob Storage service** (`Services/BlobStorageService.cs`) and a new `ImagesController`:
  - `POST /api/events/{id}/banner` (Organiser who owns the event) uploads a banner (JPG, PNG or WEBP, up to 5 MB)
  - `GET /api/events/{id}/banner` returns the banner image
  - `POST /api/users/me/profile-picture` (any logged-in user) uploads a profile picture
  - `GET /api/users/{id}/profile-picture` (any logged-in user) returns the picture
- **Enrolment status:** new `status` column on Enrolment (Pending by default), returned in enrolment responses, and `PUT /api/events/{id}/enrolments/{enrolmentId}/confirm` (Organiser who owns the event) sets it to Confirmed.
- **Results:** `GET /api/results/me` now also returns the event date, the category name and the total number of finishers, so the web app can show "47th out of 312 finishers".
- **Image URLs** are included in event and user responses.
- **Database:** three new EF Core migrations add the image URL columns and the enrolment status column. Run `Update-Database` to apply them.
- All 43 Part 2 unit tests still pass.

### Part 3: Setup Instructions

**Requirements:** Visual Studio 2022, .NET 8 SDK, SQL Server (Express or LocalDB), and an Azure Storage account with a **private** container named `raceday-images`.

1. Clone this repository and switch to the `part-3-mvc` branch.
2. Open `RaceDay.Api/RaceDay.Api.sln` in Visual Studio. It contains the API, the tests and the web app.
3. Set the SQL Server instance in `RaceDay.Api/appsettings.json` (see the Part 2 setup instructions), then run `Update-Database` in the Package Manager Console (default project `RaceDay.Api`).
4. Save the Azure connection string in the API's user secrets (it is not stored in the repository):
   `dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>" --project RaceDay.Api`
5. Right-click the solution > **Configure Startup Projects**, choose **Multiple startup projects**, and set **RaceDay.Api** and **RaceDay.Web** to **Start**. Press **F5**.
6. The API runs on `https://localhost:7152` (Swagger at `/swagger`) and the web app on `https://localhost:7006`. If your API uses a different port, change `ApiSettings:BaseUrl` in `RaceDay.Web/appsettings.json`.
7. On the web app, use **Register** to create one Organiser and one Participant and try both roles.

### Docker

The MVC app has a multi-stage `Dockerfile` in `RaceDay.Web` (the .NET SDK image to build and publish, the smaller ASP.NET runtime image to run, and a non-root user). The API keeps running on the host machine, so the container reaches it through `host.docker.internal`.

1. Run the API so the container can reach it:
   `dotnet run --project RaceDay.Api --urls "https://0.0.0.0:7152"`
2. Build and run the web app (from the `RaceDay.Web` folder):

```
docker build -t raceday-web .
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=https://host.docker.internal:7152 -e ApiSettings__AllowUntrustedCertificate=true raceday-web
```

3. Open `http://localhost:8080`.

`ApiSettings__AllowUntrustedCertificate=true` is only for running in Docker on a development machine, because the container does not trust the API's local development certificate. It is off by default.

> Note: I could not get Docker Desktop installed and running on my machine in time, so I wrote the Dockerfile following best practice but did not manage to run the container myself.

### Part 3 CI/CD

The workflow `.github/workflows/ci.yml` runs on every push to `main` and `part-3-mvc`. It validates the repository structure (now including the MVC project), builds the API and runs all unit tests, and builds the MVC project.

**Latest build status:**

![Part 3 CI/CD successful build](docs/CI-PART3.png)
### Part 3 Video Walkthrough

An unlisted YouTube video demonstrating the running application, the MVC structure, the Blob Storage integration and the Docker setup:

(https://youtu.be/8ehLo2u0sRw)

## AI Assistance Disclosure

### Part 1

AI assistance (Claude, by Anthropic) was used throughout the planning stage of this project. Specifically, it was used to:

- Explain unfamiliar concepts (e.g. 401 vs 403 HTTP status codes, SQL Server reserved words)
- Talk through and validate design decisions for the ERD (entity choices, relationship cardinality) and the API endpoint plan, based on the functional requirements in the brief
- Help troubleshoot GitHub repository setup, folder structure issues, and the GitHub Actions CI/CD workflow
- Draft an initial version of the SQL script structure, which was then reviewed, tested, and run personally in SSMS against a clean instance to confirm it works correctly

All final design decisions, the analysis of trade-offs (e.g. single User table vs separate Organiser/Participant tables), testing, and verification were done by me. I can explain and defend every choice made in this submission.

### Part 2

AI assistance (Claude, by Anthropic) was used in Part 2 for step-by-step guidance and for help with code, including the model classes, the DbContext, the controllers and the unit tests. I reviewed the code I submitted and I can explain it.

### Part 3

AI assistance (Claude, by Anthropic) was used in Part 3 for step-by-step guidance and for help with code, including setting up Azure Blob Storage, the API changes (image upload endpoints, enrolment status and results), the MVC controllers, views and styling, the Dockerfile, and the CI workflow. I typed the code into the project myself, ran and tested the application as both an Organiser and a Participant, committed the work step by step, and I can explain it.

---
© 2026 — PROG6212 Portfolio of Evidence, Parts 1, 2 and 3
