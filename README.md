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

Click **Authorize**, use the **Keycloak** option, and sign in with a local test
user. Swagger obtains the access token through Authorization Code with PKCE
and sends it with requests. The **Bearer** option also accepts an existing
access token. See the [Swagger sign-in recovery notes](docs/swagger-authentication.md)
to recreate the Keycloak client when restoring the development environment.

All Work Items, Reporting, and Applications endpoints require a valid Keycloak
access token. Requests without authentication return `401 Unauthorized`;
authenticated callers without the required role receive `403 Forbidden`.
Use the signed-in SPA to check Work Items and Reporting. Direct API requests
must include a bearer access token with the appropriate role. See the
[authorization role mapping](docs/authorization-role-mapping.md) for endpoint permissions.

## Goals

- Build a single-page application that reads and updates data through REST APIs.
- Build a reporting experience backed by a relational database.
- Practice a clean separation between browser, API, external integrations, and database access.
- Learn an end-to-end development workflow: planning, implementation, testing, containers, CI, and release.

## Current architecture

- **Frontend:** React and TypeScript single-page application
- **Backend:** ASP.NET Core Web API
- **Database:** SQL Server running locally in Docker
- **External services:** Live Atlassian Assets API in a personal development environment containing synthetic application records; local provider fakes for automated tests
- **Background processing:** Optional application synchronization in the API host, with shared coordination for manual and scheduled runs
- **Identity:** Keycloak OIDC sign-in, JWT bearer authentication, and API-enforced Reader/Admin authorization
- **Tests:** Frontend unit tests and backend API/unit tests

The browser handles interaction and presentation. The API validates requests,
enforces authorization, integrates with external services, and owns database
access and reporting queries. [ADR 0001](docs/adr/0001-use-a-backend-for-frontend.md)
describes this backend-for-frontend role. Keycloak sign-in runs in the SPA,
which holds the user's access token and forwards it with API requests; the API
validates the token and enforces the
[endpoint role policies](docs/authorization-role-mapping.md).

## Implemented features

- Work Items API and React interface for listing, creating, deleting, and updating work item status.
- SQL Server persistence through Entity Framework Core.
- Server-side Atlassian Assets integration for importing application records from a personal development environment with fake data.
- Repeatable application synchronization that preserves local identity, updates descriptive fields, and records the last successful refresh time for each imported record.
- Applications API for reading the stored copy and requesting manual synchronization. Stored reads do not depend on a successful provider request.
- Optional background synchronization with an initial run and a five-minute interval. Manual and background runs share overlap protection within one API process.
- Background synchronization logs recoverable HTTP, JSON, database, and non-shutdown cancellation failures, then retries on the next scheduled run. Shutdown cancellation releases the gate without logging an error.
- Optional application associations when creating work items through the API, validated against the configured workspace. Application selection and an Applications view remain pending in the React interface.
- Database-backed status-count report displayed in a semantic table.
- Navigation between Work Items and Reporting. Opening Reporting fetches current counts.
- Loading and error handling for frontend data requests.
- Interactive API documentation through Swagger UI.
- Backend API tests and frontend component tests, including report loading and display of zero counts.
- Keycloak SPA sign-in and access-token refresh before Work Items and Reporting requests.
- API authorization policies: Reader/Admin access for reads and Admin-only access for work item mutations and application synchronization.
- Authorization integration tests for all eight endpoints, including proof that denied sync requests do not call the external provider.

The application import currently reads one bounded provider page and retains
stored records absent from later responses. The
[learning roadmap](docs/roadmap.md) describes these milestones and remaining work.

## Planned features

The next learning chapter focuses on coordinated external write-back: queued
updates, visible operation status, retries, and reconciliation with synchronized
data. These capabilities are planned. See the [learning roadmap](docs/roadmap.md)
for the sequence, learning objectives, and open design choices.

Applications UI integration and work item application selection remain planned.
Summary cards, charts, navigation for multiple reports, and CSS/UI-framework work
remain later topics.

## Project scope

OpsPortal models generic operations workflows for personal learning and
portfolio demonstrations. Development uses synthetic records and personal
development environments. Employer systems, data, credentials, endpoints, and
business logic are outside the project scope.

## License

OpsPortal is licensed under the [MIT License](LICENSE).
