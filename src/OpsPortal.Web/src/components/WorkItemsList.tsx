import { useState } from "react";
import type { WorkItem, WorkItemStatus } from "../models/WorkItem";

type WorkItemsListProps = {
    workItems: WorkItem[];
    onDelete: (id: string) => Promise<void>;
    onStatusChange: (
        id: string,
        status: WorkItemStatus
    ) => Promise<void>;
}

export function WorkItemsList({
    workItems,
    onDelete,
    onStatusChange
}: WorkItemsListProps) {
    const [deletingId, setDeletingId] = useState<string | null>(null)
    const [errorMessage, setErrorMessage] = useState<string | null>(null)
    const [updatingStatusId, setUpdatingStatusId] = useState<string | null>(null)

    async function handleDelete(id: string) {
        setErrorMessage(null)
        setDeletingId(id)

        try {
            await onDelete(id)
        } catch (error: unknown) {
            setErrorMessage(
                error instanceof Error
                    ? error.message
                    : "An unknown error occurred"
            )
        } finally {
            setDeletingId(null)
        }
    }

    async function handleStatusUpdate(
        id: string,
        status: WorkItemStatus
    ) {
        setErrorMessage(null)
        setUpdatingStatusId(id)

        try {
            await onStatusChange(id, status)
        } catch (error: unknown) {
            setErrorMessage(
                error instanceof Error
                    ? error.message
                    : "An unknown error occurred"
            )
        } finally {
            setUpdatingStatusId(null)
        }

    }
    return (
        <>
            {errorMessage && (
                <p role="alert">{errorMessage}</p>
            )}
            <div className="table-container">
                <table className="data-table work-items-table">
                    <caption>Work Items</caption>

                    <thead>
                        <tr>
                            <th scope="col">Title</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>

                    </thead>

                    <tbody>
                        {workItems.map((workItem) => (
                            <tr key={workItem.id}>
                                <td>{workItem.title}</td>
                                <td>
                                    <select
                                        value={workItem.status}
                                        onChange={(event) =>
                                            void handleStatusUpdate(
                                                workItem.id,
                                                event.currentTarget.value as WorkItemStatus
                                            )
                                        }
                                        disabled={updatingStatusId !== null}
                                        aria-label={
                                            updatingStatusId === workItem.id
                                                ? `Updating status for ${workItem.title}`
                                                : `Status for ${workItem.title}`
                                        }
                                    >
                                        <option value="Open">Open</option>
                                        <option value="InProgress">In Progress</option>
                                        <option value="Completed">Completed</option>
                                    </select>
                                </td>
                                <td>
                                    <button
                                        type="button"
                                        onClick={() => void handleDelete(workItem.id)}
                                        disabled={deletingId !== null}
                                        aria-label={
                                            deletingId === workItem.id
                                                ? `Deleting ${workItem.title}`
                                                : `Delete ${workItem.title}`
                                        }
                                    >
                                        {deletingId === workItem.id
                                            ? `Deleting...`
                                            : `Delete`}
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </>
    )
}
