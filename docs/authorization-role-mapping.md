# Authorization Role Mapping

This is the implemented API role mapping for the Keycloak setup. The SPA signs
in through OIDC and sends bearer access tokens to the API. JWT bearer
authentication validates those tokens, and Keycloak realm roles are mapped to
role claims for authorization.

Swagger UI can also sign in through Keycloak using the `opsportal-swagger`
public client. It obtains a bearer access token and uses the same API policies
as the SPA. Client configuration and restore checks are in the
[Swagger sign-in recovery notes](swagger-authentication.md).

`CanRead` requires an authenticated Reader or Admin. `CanWrite` requires an
authenticated Admin.

| Role | Access |
| --- | --- |
| `Reader` | Read work items, the work item status report, and applications. |
| `Admin` | All work item endpoints, read access to the report and applications, and application synchronization. |

## Endpoint mapping

| Endpoint | Allowed role |
| --- | --- |
| `GET /api/work-items` | `Reader` or `Admin` |
| `GET /api/work-items/{id}` | `Reader` or `Admin` |
| `POST /api/work-items` | `Admin` |
| `PATCH /api/work-items/{id}` | `Admin` |
| `DELETE /api/work-items/{id}` | `Admin` |
| `GET /api/reports/workItemStatus` | `Reader` or `Admin` |
| `GET /api/applications` | `Reader` or `Admin` |
| `POST /api/applications/sync` | `Admin` |

The API enforces access at each endpoint. UI visibility is not an access
control. The background sync worker does not use a user role; no system-only
role is currently needed.

## Verification

Authorization integration tests cover all eight endpoints with four caller
cases: anonymous, authenticated without a role, Reader, and Admin.

- Anonymous requests return `401 Unauthorized`.
- Authenticated requests without an allowed role return `403 Forbidden`.
- Reader and Admin requests succeed on read endpoints.
- Only Admin requests succeed on work item mutations and application synchronization.
- Denied synchronization requests make zero external-provider calls; an allowed
  Admin request makes one call to the fake provider.

Tests use a test-only authentication handler while retaining the API's real
authorization policies. Work Items tests use an in-memory store; Reporting and
Applications tests use isolated SQLite databases, and synchronization uses a
fake provider. The passing suite verifies endpoint authorization without a live
Keycloak instance or external provider.

Manual SPA checks separately verified Keycloak bearer authentication and role
mapping: Reader access to Work Items and Reporting, rejected Reader mutations,
and successful Admin reads and mutations. The test handler does not validate JWT
signatures, issuer, audience, or the real realm-role transformation.
