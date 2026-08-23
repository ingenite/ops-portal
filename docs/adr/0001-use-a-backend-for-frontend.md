# ADR 0001: Use a backend-for-frontend API

- **Status:** Accepted
- **Date:** 2026-08-22

## Context

OpsPortal will be a browser-based single-page application that eventually reads
and updates data from external REST APIs and a relational database. The browser
must not receive external-service credentials or direct database connectivity.
Different external services will also have different request, response, and
authentication conventions.

## Decision

Use a React single-page application as the browser client and an ASP.NET Core
Web API as a backend-for-frontend (BFF). The SPA will call the API; the API will
own database access, external-service integration, credential handling,
validation, and API-to-UI data-shape adaptation.

## Alternatives considered

### SPA calls external APIs directly

This reduces backend code but exposes a larger browser-facing integration
surface and is not suitable for protecting service credentials or composing
multiple data sources.

### SPA connects directly to the database

This is not appropriate for a browser application. It exposes database access
and places query, authorization, and reporting logic in the client.

### Server-rendered web application

This would be a valid architecture, but it does not meet the learning goal of
building and operating a separate SPA and API.

## Consequences

The project has an additional application to develop, test, and deploy. In
return, the API provides a stable boundary for the SPA, keeps credentials
server-side, centralizes validation and authorization, and allows external
integrations or persistence implementations to change without directly
affecting the browser client.