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

### Implementation clarification (2026-10-12)

Here, BFF describes the API's integration and data-shaping role for the React
client. The browser owns user interaction and presentation; the API owns
request validation, authorization, external-service integration, persistence,
and database-backed reporting. External-service credentials stay in the API.

User authentication uses Keycloak OIDC in the SPA. The browser holds the user's
access token and sends it as a bearer token with API requests. The API validates
the JWT and enforces Reader/Admin authorization policies. This implementation
uses browser-held tokens rather than an API-managed cookie session with
server-side user-token storage. See the
[authorization role mapping](../authorization-role-mapping.md) for the current
endpoint permissions.

This clarification records the current implementation without changing the
original SPA/API boundary decision.

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
return, the API provides a stable boundary for the SPA, keeps external-service
credentials server-side, centralizes validation and authorization, and allows
external integrations or persistence implementations to change without directly
affecting the browser client.
