# Skill map

Seeded 2026-11-25 from Git history and earlier mentoring sessions. The restart sprint's baseline assessment confirms or corrects these levels.

## Levels

| Level | Meaning |
|---|---|
| — | Not started |
| exposed | Seen it explained or used; can't produce it yet |
| guided | Has done it with hints, a skeleton, or a provided solution |
| independent | Has done it solo (no hints beyond exact syntax) in a real change |
| explains | Can explain the behavior and tradeoffs without notes |

A level moves only with evidence: a commit, an issue, or a checkpoint from a dated session. Use the highest hint rung needed as a guide: H4 caps at guided; a solo milestone supports independent.

## C# language

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| Classes, records, properties | guided | Models, contracts | |
| Nullable reference types (`?`, `!`) | guided | 2026-08-23, 2026-10-12 (CS8603) | |
| String interpolation and format specifiers | guided | 2026-08-23 | |
| Lambdas and delegates | guided | 2026-08-24 | Arrow functions in JS were the bridge |
| LINQ operators and predicates | guided | 2026-08-26 `SqlWorkItemStore` | Leaned on T-SQL mental model at first |
| `async`/`await` and `Task` | guided | Stores, clients, tests | |
| Exceptions: catch, filters, wrapping | guided | f3213ff | |
| Pattern matching and switch expressions | exposed | 2026-10-14 theory, 2026-10-17 analyzers | |
| Generics | exposed | | |
| Interfaces for substitution | guided | `IWorkItemStore`, `IAtlassianAssetsClient` | |

## ASP.NET Core

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| Controllers, routing, status codes | guided | Work Items API | |
| Dependency injection and lifetimes | guided | 9322c4c, 2026-10-14 | `IServiceScopeFactory` needed a solution |
| Configuration, options, user secrets | guided | Atlassian options, README setup | |
| Hosted services and cancellation | guided | 9322c4c, f3213ff | |
| JWT bearer auth, claims, policies | guided | e05b3eb, b23b2b5 | |
| OpenAPI and Swagger | guided | a95370b | |
| Typed `HttpClient` integrations | guided | 1b87c96 | |

## EF Core and SQL

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| DbContext, entities, keys, relationships | guided | Migrations, 1079c40 | |
| Migrations | guided | Three migrations | |
| Query tracking and `AsNoTracking` | guided | `SqlWorkItemStore` | |
| T-SQL basics | independent | Prior experience | Confirm in baseline |
| Indexes and query plans | — | | Data and reporting phase |
| Transactions and optimistic concurrency | — | | Write-back phase |

## Testing

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| xUnit facts, theories, arrange/act/assert | independent | 2026-08-23 unhappy-path tests, 2026-10-14 theory refactor | Simple cases |
| `WebApplicationFactory` integration tests | guided | `OpsPortalApiFactory` | |
| Test doubles and fakes | guided | 4c503f5 | |
| Async coordination (`TaskCompletionSource`, `FakeTimeProvider`) | exposed | 2026-10-14 | Full solution needed |
| SQLite-backed data tests | guided | 8f4e3a7 | |
| Testcontainers / real SQL Server tests | — | | Ship phase |

## TypeScript and React

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| JavaScript fundamentals | guided | Eloquent JavaScript labs | Confirm in baseline |
| TypeScript types and interfaces | guided | `models/` | |
| Components and props | guided | `WorkItemsList`, report view | |
| State and effects | guided | `App.tsx` | |
| Forms and controlled inputs | guided | `CreateWorkItemForm` | |
| Fetching, loading, and error states | guided | `api/` | |
| Vitest and Testing Library | guided | Component tests | |
| SPA authentication (keycloak-js) | guided | f80638e | |
| Client-side routing | — | | Frontend phase |

## HTML and CSS

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| Semantic HTML | guided | Report table | |
| CSS layout (flexbox, grid) | — | | Frontend phase |
| Accessibility basics | exposed | Issue labels | |

## Delivery and tooling

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| PowerShell toolmaking | independent | `OpsPortal.psm1` | |
| Git solo workflow, branches, PRs | guided | 2026-10-15 first PR | Prior GitLab MR experience |
| GitHub Actions | guided | a0dcd3d | |
| Docker and Compose | guided | `infra/compose.yaml` | |
| Package updates and vulnerability checks | guided | 9085ea3 | |
| Analyzers and formatting | guided | 54a3924 | |

## Design and engineering practice

| Skill | Level | Evidence | Notes |
|---|---|---|---|
| Layer boundaries (BFF) | guided | ADR 0001 | |
| Writing ADRs | exposed | ADR 0001 | |
| Estimating work | — | | Starts in restart sprint |
| Messaging, retries, idempotency | — | | Write-back phase |
| Service decomposition and data ownership | — | | Microservices phase |
