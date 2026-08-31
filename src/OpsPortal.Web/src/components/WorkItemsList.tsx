import { useState } from "react";
import type { WorkItem } from "../models/WorkItem";

type WorkItemsListProps = {
    workItems: WorkItem[];
    onDelete: (id: string) => Promise<void>;
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

            <ul>
                {workItems.map((workItem) => (
                    <li key={workItem.id}>
                        {workItem.title} - {workItem.status}

                        <button
                            type="button"
                            onClick={() => void handleDelete(workItem.id)}
                            disabled={deletingId !== null}
                        >
                            {deletingId === workItem.id
                                ? "Deleting..."
                                : "Delete"}
                        </button>
                    </li>
                ))}
            </ul>
        </>
    )
}