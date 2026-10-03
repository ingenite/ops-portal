import type { WorkItem } from "../../models/WorkItem";

export function createWorkItem(overrides: Partial<WorkItem> = {}): WorkItem {
  return {
    id: "work-item-1",
    title: "Review API contract",
    status: "Open",
    createdAtUtc: "2026-09-03T00:00:00Z",
    updatedAtUtc: "2026-09-03T00:00:00Z",
    ...overrides,
  };
}
