# 🔧 Scheduler Architecture

This project aims to demonstrate the fundamental skills required for creating a Web API.

The project follows the classic **3-tier architecture**, which ensures a clear separation of concerns, scalability, and maintainability.

[![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?logo=swagger&logoColor=white)](https://scheduler-api-demo.azurewebsites.net/swagger/index.html)
---

## Project Structure

| Layer            | Project Name   | Responsibility                                      |
|------------------|----------------|-----------------------------------------------------|
| **Presentation** | `Scheduler.WebApi`     | Web layer: controllers and settings. |
| **Business Logic** | `Scheduler.BLL`       | Core logic: DTOs and services.    |
| **Data Access**  | `Scheduler.DAL`        | Database access: Entities and DbContext. |
| **Tests**        | `Scheduler.Tests`      | Integration and Unit tests.                 |

---

## 🗂 Layer Overview

- `Scheduler.WebApi` – The entry point. Handles routing and controllers.
- `Scheduler.BLL` – Business logic: coordinates service operations and transforms data using DTOs.
- `Scheduler.DAL` – Data access logic using EF Core: includes entities and DbContext.
- `Scheduler.Tests` – Verifies real and mocked interactions with the in-memory database.

---

## 📐 Architecture Diagram

```mermaid
graph TD
    A[Scheduler.WebApi<br/>Presentation Layer] --> B[Scheduler.BLL<br/>Business Logic Layer]
    B --> C[Scheduler.DAL<br/>Data Access Layer]
    T[Scheduler.Tests<br/>Tests] --> B
    subgraph DB
        D[(In Memory Database)]
    end
    C --> D
```

---

## 🔧 How to Run

1. 📦 **Install .NET 9 SDK**  
   👉 [Download .NET 9 SDK](https://dotnet.microsoft.com/en-us/download)

2. 📥 **Clone the repository**
   ```bash
   git clone https://github.com/serhiiyasenev/Scheduler.git
   cd Scheduler
   ```

3. 🧱 **Build the solution**
   ```bash
   dotnet build Scheduler.sln
   ```

4. 🧪 **Run the tests**
   ```bash
   dotnet test Scheduler.sln
   ```

5. 🚀 **Run the Web API**
   ```bash
   dotnet run --project Scheduler.WebApi
   ```

6. 🌐 **Open Swagger UI**  
   👉 [https://localhost:7272/swagger/index.html](https://localhost:7272/swagger/index.html)

---
   ## 🛢️ Database Diagram

```mermaid
erDiagram
    USER {
        guid id
        string name
        string nameNormalized
        string email
    }

    MEETING {
        guid id
        string title
        string description
        datetime startAt
        datetime endAt
        string location
        string status
        guid organizerId
    }

    MEETING_PARTICIPANT {
        guid meetingId
        guid userId
        string role
        boolean isRequired
        string responseStatus
    }

    %% Relationships
    USER    ||--o{ MEETING : organizes
    MEETING ||--o{ MEETING_PARTICIPANT : has
    USER    ||--o{ MEETING_PARTICIPANT : attends

    %% Notes (comments only, not parsed):
    %% - Unique index: USER.nameNormalized
    %% - Composite PK: MEETING_PARTICIPANT (meetingId, userId)
    %% - FKs: MEETING.organizerId -> USER.id; MEETING_PARTICIPANT.(meetingId->MEETING.id, userId->USER.id)
```
