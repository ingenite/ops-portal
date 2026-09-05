import type { WorkItemStatus } from "./WorkItem"

export type WorkItemsByStatusReportRow = {
    status: WorkItemStatus;
    count: number;
}
