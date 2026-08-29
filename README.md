# OpsPortal

A personal learning project that models the architecture and delivery practices of a modern internal web application.

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

> Coming soon - Powershell script using Invoke-RestMethod

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
- **External services:** Mocked SaaS-style REST API initially; designed to be replaceable with real integrations
- **Tests:** Frontend unit tests and backend API/unit tests

## Initial features

1. An API integration page that demonstrates reading and creating or updating data.
2. A reporting page that displays database-backed results in a table.
3. Summary cards and simple charts.
4. Report navigation that scales to multiple reports.

## Non-goals

- This is not an operational EUC tool.
- This project will not connect to employer systems, data, or credentials.
- Authentication and authorization are deferred until the core application shape is established.