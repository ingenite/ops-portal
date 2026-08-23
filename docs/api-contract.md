# API Contract

This document defines the initial HTTP API surface for OpsPortal.

## Conventions

- Base API path: `/api`
- Request and response bodies use JSON.
- Resource IDs are server-generated GUIDs.
- Timestamps use UTC ISO 8601 values.
- This initial version has no authentication or authorization.

## Work items

A work item is a generic, trackable record used to demonstrate REST API operations.

### Work item representation

```json
{
  "id": "e8f2f65b-6fa6-4f6d-8cab-9f59fa4a64f7",
  "title": "Review integration documentation",
  "status": "Open",
  "createdAtUtc": "2026-08-22T20:00:00Z",
  "updatedAtUtc": "2026-08-22T20:00:00Z"
}
```

Allowed initial status values are `Open`, `InProgress`, and `Completed`.

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
  "title": "Review integration documentation"
}
```

Responses:

- `201 Created` with the created work item and a `Location` header.
- `400 Bad Request` when the request fails validation.

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