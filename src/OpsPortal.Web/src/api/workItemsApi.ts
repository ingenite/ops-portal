import type { WorkItem } from "../models/WorkItem";

export async function getWorkItems(): Promise<WorkItem[]> {
    const response = await fetch('/api/work-items')

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const apiWorkItems: WorkItem[] = await response.json();
    return apiWorkItems    
};