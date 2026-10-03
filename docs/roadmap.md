# Learning roadmap

## Purpose and working approach

OpsPortal reinforces technical-book learning through a practical application.
The goal is to develop enough understanding to direct AI-assisted work, assess
design choices, review code, and verify behavior across layers. Independent
recall of every language or library API is not the goal.

Work proceeds in small, reviewable units. Documentation lookup and asking for
help are part of the learning process. When unfamiliar syntax or API composition
blocks progress, use a focused example and explanation. Testing accompanies
meaningful behavior changes, with depth balanced against the current lesson.

The Work Items CRUD and initial status-count reporting slices are complete.
The following chapters extend that foundation. This is a learning sequence,
not a commitment to a particular queue product, identity provider, or deployment
topology.

## 1. Read from an external provider

Validate a personal Atlassian demo development site as a possible source.
Confirm Assets availability, suitable fake records, API access, and environment
expiry before depending on it. The exact dataset remains open.

Build one small read path through the ASP.NET Core API. Learn outbound HTTP,
configuration, provider response types, and mapping into application-owned
response shapes. Keep provider credentials on the server.

Checkpoint: a bounded set of fake external records can be read through OpsPortal
and checked against the source. Keep a local fake available for controlled tests.

## 2. Persist a synchronized copy

Store external records in SQL Server using stable external identifiers. Learn
repeatable imports, source-to-local mapping, and freshness information. Establish
which system owns each field and what the local copy represents.

Checkpoint: repeating an import updates the same records without creating
duplicates, and the application can show when its local data was refreshed.

## 3. Introduce background synchronization

Move the import operation into background processing. Learn worker lifetimes,
dependency scopes, cancellation, logging, and how to prevent unintended overlap.
Decide whether the worker initially runs in the API host or a separate process.

Checkpoint: synchronization runs without a browser request, failures are visible,
and shutdown and overlapping runs have defined behavior.

## 4. Add authentication and authorization (complete)

Implemented Keycloak OIDC sign-in in the SPA and JWT bearer authentication in
the API. The browser refreshes its access token before Work Items and Reporting
requests. The API maps Keycloak realm roles to role claims and applies `CanRead`
(Reader or Admin) and `CanWrite` (Admin) policies across all eight endpoints.

This chapter covered the application side of OIDC/OAuth2, token validation,
claims, and authorization policies. User authorization remains separate from
the background worker's credentials and permissions at the external provider.

Checkpoint completed: anonymous callers receive `401`, authenticated callers
without an allowed role receive `403`, Readers can read, and only Admins can
mutate work items or request application synchronization. The API enforces these
rules independently of which controls React displays.

Integration tests cover all four caller cases on every endpoint and verify that
denied sync requests make no provider calls. Manual SPA checks verified the real
Keycloak flow for Work Items and Reporting. Test authentication substitutes for
JWT validation in integration tests; it does not exercise real token validation
or Keycloak realm-role mapping. See the
[authorization role mapping](authorization-role-mapping.md) for details.

## 5. Coordinate external write-back

Accept an update through the API, persist the requested work, and process it in
the background. Expose operation status so the UI can distinguish acceptance,
execution, failure, and reconciliation with the local reporting copy. Preserve
the requesting user's identity for traceability.

Learn durable work records, retries, duplicate execution handling, and recovery
after interruption. A SQL-backed queue is a candidate; a separate message broker
is not yet a requirement. Define when authorization is checked for queued work.

Coordinate scheduled synchronization with user updates. A queue alone does not
prevent an older sync snapshot from overwriting newer local information. Explore
ordering or stale-update detection, and account for providers whose reads take
time to reflect an accepted write.

Checkpoint: a fake update overlapping a sync has a predictable outcome, the UI
shows accurate operation status, and a retry or restart does not silently lose
the request or apply an unintended duplicate effect.

## Architectural boundaries

- React owns interaction, presentation, and UI state.
- The application API validates requests and enforces user authorization.
- Provider-specific integration code handles external API communication.
- The data layer handles SQL persistence and queries.
- Background processing performs synchronization and queued operations through
  those integration and persistence capabilities.
- Reports read the local synchronized copy; operation status explains work that
  has not yet been reflected in that copy.

These boundaries extend [ADR 0001](adr/0001-use-a-backend-for-frontend.md).
Record concrete decisions about identity integration, queue persistence, or
coordination in new ADRs when the relevant chapter makes those choices.

## Later topics

- Summary cards and charts driven by useful reporting questions.
- Navigation that scales to multiple reports.
- CSS and UI frameworks such as MUI.
- Broader testing, CI, deployment, and operational practices as the application
  requires them.

## Project boundary

Use personal development environments and fake data. Employer systems, data,
credentials, and business logic remain outside OpsPortal. Familiar external
products provide context for portable integration patterns.

## Reference

- [Atlassian demo development sites](https://developer.atlassian.com/platform/forge/provision-a-demo-development-site/)
