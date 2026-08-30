import type { WorkItem, CreateWorkItemRequest } from "../models/WorkItem";

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