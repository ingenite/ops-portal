import { useState } from "react";

type CreateWorkItemFormProps = {
    onCreate: (title: string) => Promise<void>
}

export function CreateWorkItemForm({ onCreate }: CreateWorkItemFormProps) {
    const [title, setTitle] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [errorMessage, setErrorMessage] = useState<string | null>(null)

    async function handleSubmit(event: React.SyntheticEvent) {
        event.preventDefault()

        const trimmedTitle = title.trim()
        if (!trimmedTitle) {
            return
        }

        setErrorMessage(null)
        setIsSubmitting(true)

        try {
            await onCreate(trimmedTitle)
            setTitle("")
        } catch (error:unknown) {
            setErrorMessage(
                error instanceof Error
                ? error.message
                : 'An unknown error occurred'
            )
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <label>New WorkItem Title:
                <input
                    type= "text"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                    disabled={isSubmitting}
                />
            </label>

            {errorMessage && (
                <p role="alert">{errorMessage}</p>
            )}

            <button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Creating..." : "Create"}
            </button>
        </form>
    )
}
