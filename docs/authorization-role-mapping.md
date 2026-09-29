# Authorization Role Mapping

This is the intended API role mapping for the Keycloak setup. It records the
current design; authentication and authorization are not implemented yet.

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
