import { useState } from "react";

type CreateWorkItemFormProps = {
    onCreate: (title: string) => void;
}


export function CreateWorkItemForm({ onCreate }: CreateWorkItemFormProps) {
    const [title, setTitle] = useState("");

    function handleSubmit(event: React.SyntheticEvent) {
        event.preventDefault()
        
        const trimmedTitle = title.trim()
        if (!trimmedTitle)  return 

        onCreate(trimmedTitle)
    }

    return (
        <form onSubmit={handleSubmit}>
            <label>New WorkItem Title: 
                <input
                    type= "text"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                />
            </label>
            <button type="submit">Create</button>
        </form>
    )
}