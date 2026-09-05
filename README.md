# OpsPortal

A personal learning project that models the architecture and delivery practices of a modern internal web application.

The project reinforces learning from technical books through practical implementation. It also develops the knowledge needed to direct AI-assisted development: understand proposed solutions, assess architectural choices, review generated code, and verify behavior across the application.

## Local Development

### Pre-Reqs

- .NET 10 SDK
- WSL2
- Docker Desktop (requires WSL2)
- Git for Windows
- pwsh

### Initial Setup

1. In pwsh, run these commands from the repository root

    ```powershell
    Import-Module .\OpsPortal.psm1
    # Append -Overwrite to backup an existing file and create a new one
    New-EnvFile
    ```

2. Restore repo-local EF Core with `dotnet tool restore`.
3. Configure `ConnectionStrings:OpsPortalDatabase` with dotnet User Secrets

    ```powershell
    Set-dotNetUserSecret
    ```

4. Start SQL container

    ```powershell
    Start-SqlContainer
    ```

5. Apply existing EF Core migration

    ```powershell
    Invoke-EFDatabaseUpdate
    ```

### Verify API

With the API running locally, open Swagger UI at:

http://localhost:5096/swagger

Swagger UI is available in the Development environment. It displays the generated OpenAPI contract and supports executing requests against the running API.

The status-count report can also be checked from PowerShell:

```powershell
Invoke-RestMethod http://localhost:5096/api/reports/workItemStatus
```

### Build and Test

> Coming soon - Powershell script accepting various common args

## Goals

- Build a single-page application that reads and updates data through REST APIs.
- Build a reporting experience backed by a relational database.
- Practice a clean separation between browser, API, external integrations, and database access.
- Learn an end-to-end development workflow: planning, implementation, testing, containers, CI, and release.

## Planned architecture

- **Frontend:** React and TypeScript single-page application
- **Backend:** ASP.NET Core Web API
- **Database:** SQL Server running locally in Docker
- **External services:** Personal development environments with fake data, plus local fakes for repeatable failure and concurrency scenarios
- **Background processing (planned):** Synchronization and queued external updates with visible operation status
- **Identity (planned):** OIDC sign-in and API-enforced authorization before external write-back
- **Tests:** Frontend unit tests and backend API/unit tests

## Implemented features

- Work Items API and React interface for listing, creating, deleting, and updating work item status.
- SQL Server persistence through Entity Framework Core.
- Database-backed status-count report displayed in a semantic table.
- Navigation between Work Items and Reporting. Opening Reporting fetches current counts.
- Loading and error handling for frontend data requests.
- Interactive API documentation through Swagger UI.
- Backend API tests and frontend component tests, including report loading and display of zero counts.

## Planned features

The next chapters focus on external integration, background synchronization,
authentication and authorization, and coordinated write-back. See the
[learning roadmap](docs/roadmap.md) for the sequence, learning objectives, and
open design choices.

Summary cards, charts, navigation for multiple reports, and CSS/UI-framework
work remain later topics.

## Non-goals

- This is not an operational EUC tool.
- This project will not connect to employer systems, data, or credentials.
