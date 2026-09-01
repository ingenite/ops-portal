import type { WorkItem,
            CreateWorkItemRequest,
            UpdateWorkItemStatusRequest
        } from "../models/WorkItem";

export async function getWorkItems(): Promise<WorkItem[]> {
    const response = await fetch('/api/work-items')

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const apiWorkItems: WorkItem[] = await response.json();
    return apiWorkItems
};

export async function createWorkItem(request: CreateWorkItemRequest): Promise<WorkItem> {
    const response = await fetch("/api/work-items", {
        method: "POST",
        body: JSON.stringify(request),
        headers: {
            "Content-Type": "application/json"
        }
    })
    // Response.ok = true for 200-299
    if (!response.ok) {
        throw new Error(`Failed to create WorkItem: ${response.status}`)
    }

    const created: WorkItem = await response.json();
    return created
}

export async function deleteWorkItem(id: string): Promise<void> {
    const response = await fetch(`/api/work-items/${id}`, {
        method: "DELETE"
    })

    if (!response.ok) {
        throw new Error(`Could not delete item: ${response.status}`)
    }
}

export async function updateWorkItem(
    id: string,
    request: UpdateWorkItemStatusRequest
): Promise<WorkItem> {
    const response = await fetch(`/api/work-items/${id}`, {
        method: "PATCH",
        body: JSON.stringify(request),
        headers: {
            "Content-Type": "application/json"
        }
    })

    if (!response.ok) {
        throw new Error(`Failed to update WorkItem: ${response.status}`)
    }

    const updated: WorkItem = await response.json();
    return updated
}
