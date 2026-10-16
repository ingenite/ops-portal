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
Chapters 1-4 describe implemented milestones and their current limits; chapter 5
is future work. This is a learning sequence, not a commitment to a particular
queue product or deployment topology.

GitHub issues track backlog work, starting with
[issue #1: sync worker failure recovery](https://github.com/ingenite/ops-portal/issues/1).

## 1. Read from an external provider (complete)

Implemented a server-side Atlassian Assets client for application records in a
personal development environment with fake data. Provider response types and a
mapper translate the external schema into application-owned fields. Provider
credentials remain on the server.

This chapter covered outbound HTTP, configuration, JSON deserialization, and
mapping across the provider/application boundary. The current client reads one
bounded page; pagination remains outside the implemented read path.

Checkpoint completed: provider reads feed application synchronization through
the API. Tests cover the HTTP request, deserialization, and mapping with fake
responses. Test fakes support controlled scenarios; a runnable local fake-provider
mode for visitors remains future work.

## 2. Persist a synchronized copy (complete)

Implemented application storage through EF Core and SQL Server. Synchronization
matches records by workspace and external object ID, preserves local identity,
updates descriptive fields, and assigns one UTC refresh timestamp to each
imported batch. Records absent from a later response remain stored with their
previous refresh timestamp.

`GET /api/applications` reads the stored copy for the configured workspace and
includes each record's local ID and `lastSyncedAtUtc`. Reads do not call the
provider, so previously synchronized data remains available during a provider
failure. Work item creation accepts an optional local `applicationId`, validated
against that workspace. An Applications view and application selection in the
React interface remain future work.

This chapter covered repeatable imports, source-to-local mapping, relational
identity, and freshness. Checkpoint completed: tests verify that repeated imports
preserve local IDs without creating duplicates, update mapped values and
timestamps, isolate workspaces, and preserve records absent from the response.
API tests verify stored reads and manual synchronization. These tests use SQLite
and fake providers; they do not verify SQL Server migrations or live Atlassian
behavior.

## 3. Introduce background synchronization (complete)

Implemented an optional background worker in the API host, enabled through
`ApplicationSync:Enabled`. It attempts synchronization immediately and on a
five-minute timer, creates a fresh dependency scope for each acquired run, and
passes the shutdown cancellation token into synchronization.

Manual and background runs share a coordinator within one API process. A manual
request receives `409 Conflict` when the gate is held; the worker skips an attempt
when another run holds the gate. Both paths release an acquired gate in `finally`.
This coordination does not extend across multiple API instances.

This chapter covered worker lifetimes, dependency scopes, cancellation, logging,
and overlap protection. The worker logs HTTP request, JSON, database, EF Core
save, and non-shutdown cancellation failures, releases the gate, and attempts
synchronization again on the next scheduled tick with a fresh dependency scope.
Shutdown cancellation during synchronization exits without an error log and
releases the gate. Exception handling is limited to these recoverable categories;
unexpected exceptions still propagate.

Checkpoint completed: tests cover gate acquisition/release and manual-sync
conflict behavior. [Worker tests](../tests/OpsPortal.Api.Tests/ApplicationSyncWorkerTests.cs)
exercise each recoverable failure category followed by a successful scheduled
run that persists application data. An injected `TimeProvider` lets tests advance
the five-minute schedule without waiting in real time. A separate test verifies
shutdown during an active sync, successful worker completion, gate release, and
no error log for shutdown cancellation. These tests use fake provider failures
and SQLite; they do not verify live Atlassian or SQL Server failure behavior.

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
- Background processing currently performs synchronization through the
  integration and persistence capabilities. Queued operations are planned in
  chapter 5.
- The current status-count report reads stored work items, and the Applications
  API reads the local synchronized copy. Future operation status will explain
  queued work that has not yet been reflected in that copy.

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
