# Scheduler Architecture

This is a learning process that aims to demonstrate the basic skills of creating a Web API, the project follows the classic **3-tier architecture**, which ensures a clear separation of concerns, scalability, and maintainability.

---

## Project Structure

| Layer            | Project Name   | Responsibility                                      |
|------------------|----------------|-----------------------------------------------------|
| **Presentation** | `Scheduler.WebApi`     | Web layer: controllers, and settings. |
| **Business Logic** | `Scheduler.BLL`       | Core logic: DTOs and services.    |
| **Data Access**  | `Scheduler.DAL`        | Database access: DbContext and In Memory Database. |
| **Tests**        | `Scheduler.Tests`      | Integration and Unit tests.                 |

---

## 🗂 Layer Overview

- `Scheduler.WebApi` – The entry point. Handles routing and controllers.
- `Scheduler.BLL` – Business logic: coordinates service operations and transforms data using DTOs.
- `Scheduler.DAL` – Data access logic using EF Core: includes entities and DbContext.
- `Scheduler.Tests` – Verifies real and mocking interactions with the in-memory database.

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
