import { useState } from "react";
import type { WorkItem } from "../models/WorkItem";

type WorkItemsListProps = {
    workItems: WorkItem[];
    onDelete: (id: string) => Promise<void>
}

export function WorkItemsList({
    workItems,
    onDelete
}: WorkItemsListProps) {
    const [deletingId, setDeletingId] = useState<string | null>(null)
    const [errorMessage, setErrorMessage] = useState<string | null>(null)

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

    return (
        <>
            {errorMessage && (
                <p role="alert">{errorMessage}</p>
            )}
            <table>
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
                            <td>{workItem.status}</td>
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
        </>
    )
}