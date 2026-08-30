// string-literal union
type WorkItemStatus = "Open" | "InProgress" | "Completed";

// match to backend model
export type WorkItem = {
  id: string;
  title: string;
  status: WorkItemStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type CreateWorkItemRequest = {
    title: string;
};