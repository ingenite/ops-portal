import { useEffect, useState } from "react";

// string-literal union
type WorkItemStatus = "Open" | "InProgress" | "Completed";

// match to backend model
type WorkItem = {
  id: string;
  title: string;
  status: WorkItemStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
}

function App() {
  const [workItems, setWorkItems] = useState<WorkItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let ignore = false

    async function loadWorkItems() {
      try {
        // Fetch /api/work-items
        const response = await fetch('/api/work-items')

        // Check OK
        if (!response.ok) { 
          throw new Error(`status: ${response.status}`)
        }

        // Read JSON as WorkItem[]
        const apiWorkItems: WorkItem[] = await response.json();

        if (!ignore) {
          setWorkItems(apiWorkItems)
        }
      } catch (error: unknown) {
        if (!ignore) {
          setErrorMessage(
            error instanceof Error
              ? error.message
              : 'An unknown error occurred'
          )
        }
      } finally {
        if (!ignore) {
          setIsLoading(false)
        }
      }
    }

    void loadWorkItems()

    return () => {
      ignore = true
    }
  }, [])

  return (
    <main>
      <h1>Work Items</h1>
      {isLoading ? (
        <p>Loading work items...</p>
      ) : errorMessage ? (
        <p role="alert">{errorMessage}</p>
      ) : workItems.length === 0 ? (
        <p>No work items found.</p>
      ) : (
        <ul>
          {workItems.map((workItem) => (
            <li key={workItem.id}>
              {workItem.title} - {workItem.status}
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}

export default App

