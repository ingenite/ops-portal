import type { WorkItem } from "../models/WorkItem";

type WorkItemsListProps = {
    workItems: WorkItem[]
}

export function WorkItemsList({ workItems }: WorkItemsListProps) {
    return (
        <ul>
            {workItems.map((workItem) => (
                <li key={workItem.id}>
                    {workItem.title} - {workItem.status}
                </li>
            ))}
        </ul>
    )
}