# Scheduler

[![Tests](https://github.com/serhiiyasenev/Scheduler/actions/workflows/test.yml/badge.svg)](https://github.com/serhiiyasenev/Scheduler/actions/workflows/test.yml)

An ASP.NET Core API that finds a common free time for a group of participants and creates a meeting in the earliest available slot. It demonstrates a focused scheduling problem: combine participants' busy intervals, fit a requested duration into a time window, and explain when no slot is available.

**Project status:** a .NET 9 portfolio demo. It stores users and meetings in memory, so restarting the process clears the data. It is not connected to external calendars or a video-conferencing provider.

## What you can demonstrate

| Scenario | Expected behavior |
|---|---|
| Create participants | Register users and reject duplicate normalized names |
| Find a common slot | Return the earliest start that fits all participants' stored meetings |
| Reserve the slot | Create a meeting and return its start/end time and participants |
| Handle conflicts | Return `409` when the requested time window is fully occupied |
| Validate input | Reject duplicate/missing participants, invalid duration and invalid time bounds |
| Inspect a schedule | List all meetings, a specific meeting or one participant's meetings |

## Run locally

Prerequisites: .NET 9 SDK and a local HTTPS development certificate. No database server, containers or account credentials are required.

```bash
git clone https://github.com/serhiiyasenev/Scheduler.git
cd Scheduler
dotnet restore Scheduler.sln
dotnet dev-certs https --trust
dotnet run --project Scheduler.WebApi --launch-profile https
```

Open [Swagger](https://localhost:7272/swagger). The `https` launch profile fixes the port to `7272`. Start with a fresh process for the demo below; no users or meetings are seeded.

## Five-minute demo

Use Swagger to perform these requests in order. On a fresh process, the first two users normally have IDs `1` and `2`; always use the IDs actually returned by the API.

1. Call `POST /api/User/CreateUser` twice:

   ```json
   { "name": "Olena" }
   ```

   ```json
   { "name": "Taras" }
   ```

2. Call `POST /api/Meetings/FindSlot` with:

   ```json
   {
     "participantIds": [1, 2],
     "durationMinutes": 30,
     "earliestStart": "2030-01-15T09:00:00Z",
     "latestEnd": "2030-01-15T10:30:00Z"
   }
   ```

   Expect a start of `09:00`. Dates can be changed; use a single day and UTC timestamps.
3. Submit the same body to `POST /api/Meetings/CreateOrFindEarliestMeetingSlotWithSuggestions`. Expect `201` and a meeting from `09:00` to `09:30`.
4. Repeat `FindSlot`. Expect `09:30`, because both participants are now busy during the first half hour.
5. Change `latestEnd` to `2030-01-15T09:30:00Z` and repeat `FindSlot`. Expect `409` because no 30-minute slot remains.
6. Call `GET /api/Meetings/GetMeetingsByUserId/1` and inspect the reservation. Trying to create `" olena "` again demonstrates normalized-name conflict handling.

## Architecture and tradeoffs

| Project | Responsibility | Reason for the boundary |
|---|---|---|
| `Scheduler.WebApi` | Controllers, validation setup, Swagger and dependency injection | Defines the HTTP interface and maps results to status codes |
| `Scheduler.BLL` | Request validation and scheduling/user services | Holds the interval-search logic independently of HTTP |
| `Scheduler.DAL` | Users, meetings, participant joins and EF repositories | Encapsulates storage access; current provider is EF Core InMemory |
| `Scheduler.Tests` | Service, controller and validation checks | Exercises scheduling behavior and API orchestration |

The scheduling service loads overlapping meetings for the requested participants, sorts their busy intervals and advances a cursor until the meeting fits. A simple layered application keeps that logic easy to inspect. In-memory storage makes the demo quick to start, but does not provide durability or relational concurrency guarantees.

## Tests

```bash
dotnet test Scheduler.sln --configuration Release
```

Tests cover service behavior, controller responses and input validation. Persistence tests use EF Core InMemory; they do not verify real database transactions or simultaneous reservations. Pull requests targeting `main` run the [test workflow](.github/workflows/test.yml).

## Known limitations

- All data is lost when the process restarts. There is no migration or production database configuration.
- Finding a slot and saving a meeting are separate operations. Concurrent requests can race; production booking requires an atomic reservation strategy.
- The validator checks time bounds against 09:00–17:00, but the application has no timezone conversion, daylight-saving, recurring-meeting, holiday or full multi-day business-calendar model. Use UTC and a single-day window for the demo.
- There is no authentication and CORS currently allows any origin. Keep the demo isolated.
- `SuggestAvailableSlotsAsync` exists, but the HTTP fallback searches the same range as the initial failed search; it does not find alternatives outside that range.
- The generated `Link` field is a placeholder path, not a meeting room, and differs from the implemented lookup route. Use the response `Location` header or `GET /api/Meetings/GetMeetingById/{id}`.
- No Google/Outlook calendar integration, invitations or video calls are implemented.

The [Azure workflow](.github/workflows/deploy.yml) deploys pushes to `main` when repository secrets are configured. A working hosted deployment is not required for the local demo and is not asserted by this README.
