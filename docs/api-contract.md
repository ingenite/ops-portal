# API Contract

This document defines the initial HTTP API surface for OpsPortal.

## Conventions

- Base API path: `/api`
- Request and response bodies use JSON.
- Resource IDs are server-generated GUIDs.
- Timestamps use UTC ISO 8601 values.
- Endpoints require bearer token for authorization:
  - All endpoints return `401 Unauthorized` when an invalid or no token are provided.
  - All endpoints return `403 Forbidden` when the token does not carry the appropriate roles.
  - Requirements are documented in [authorization-role-mapping.md](./authorization-role-mapping.md)

## Work items

A work item is a generic, trackable record used to demonstrate REST API operations.

### Work item representation

```json
{
  "id": "e8f2f65b-6fa6-4f6d-8cab-9f59fa4a64f7",
  "title": "Review integration documentation",
  "status": "Open",
  "createdAtUtc": "2026-08-22T20:00:00Z",
  "updatedAtUtc": "2026-08-22T20:00:00Z",
  "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Allowed initial status values are `Open`, `InProgress`, and `Completed`.

`applicationId` is an optional reference to a local `Application.Id`. It is not its ObjectKey or any other Atlassian-owned id.

### Get all work items

```text
GET /api/work-items
```

Response: `200 OK` with an array of work items.

### Get one work item

```text
GET /api/work-items/{id}
```

Responses:

- `200 OK` with the requested work item.
- `404 Not Found` when no work item exists with that ID.

### Create a work item

```text
POST /api/work-items
Content-Type: application/json
```

Request body:

```json
{
  "title": "Review integration documentation",
  "applicationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

`applicationId` is optional.

Responses:

- `201 Created` with the created work item and a `Location` header.
- `400 Bad Request` when the request fails validation.
- `400 Bad Request` when the provided `applicationId` does not exist in the configured workspace.

### Partially update a work item

```text
PATCH /api/work-items/{id}
Content-Type: application/json
```

Request body example:

```json
{
  "status": "Completed"
}
```

Responses:

- `200 OK` with the updated work item.
- `400 Bad Request` when no supported fields are provided or a supplied value fails validation.
- `404 Not Found` when no work item exists with that ID.

### Delete a work item

```text
DELETE /api/work-items/{id}
```

Responses:

- `204 No Content` when the work item existed and was deleted.
- `404 Not Found` when no work item exists with that ID.

## Reports

### Work Items By Status, Count

```text
GET /api/reports/workItemStatus
```

Responses:

- `200 OK` with JSON array with 1 row per status including zero counts.
  - Ordered: Open, InProgress, Completed.
  - Counts cover all currently stored work items. An empty database returns three zero-count rows.

```json
[
  {
    "status": "Open",
    "count": 1
  },
  {
    "status": "InProgress",
    "count": 1
  },
  {
    "status": "Completed",
    "count": 1
  }
]
```

## Applications

Applications are locally stored objects synced from Atlassian Assets.

### Application representation

```json
{
  "externalId": "string",
  "key": "string",
  "name": "string",
  "deploymentMethod": "string",
  "status": "string",
  "lastSyncedAtUtc": "2026-10-10T05:04:49.453Z",
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Get all applications

```text
GET /api/applications
```

Returns all stored applications for the configured workspace.

Response: `200 OK` with an array of applications.

### Manually trigger synchronization

```text
POST /api/applications/sync
```

Responses:

- `204 No Content` sync is successful.
- `409 Conflict` when another manual or background service sync are in progress.
- `502 Bad Gateway` when contacting the external provider fails.