import type { WorkItemsByStatusReportRow } from "../models/WorkItemsByStatusReportRow";

export async function getWorkItemsByStatusReport(): Promise<WorkItemsByStatusReportRow[]> {
    const response = await fetch('/api/reports/workItemStatus')

    if (!response.ok) {
        throw new Error(`Failed to fetch report: ${response.status}`)
    }

    const apiWorkItemsByStatusReport: WorkItemsByStatusReportRow[] = await response.json();
    return apiWorkItemsByStatusReport
}
