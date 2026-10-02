import type { WorkItemsByStatusReportRow } from "../models/WorkItemsByStatusReportRow";
import keycloak from "../auth/keycloak";

export async function getWorkItemsByStatusReport(): Promise<WorkItemsByStatusReportRow[]> {
    await keycloak.updateToken();

    const response = await fetch('/api/reports/workItemStatus', {
        method: "GET",
        headers: {
            "authorization": `Bearer ${keycloak.token}`
        }
    })

    if (!response.ok) {
        throw new Error(`Failed to fetch report: ${response.status}`)
    }

    const apiWorkItemsByStatusReport: WorkItemsByStatusReportRow[] = await response.json();
    return apiWorkItemsByStatusReport
}
