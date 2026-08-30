import { useEffect, useState } from "react";
import { type WorkItem } from "./models/WorkItem";
import { getWorkItems } from "./api/workItemsApi";
import { WorkItemsList } from "./components/WorkItemsList";

function App() {
  const [workItems, setWorkItems] = useState<WorkItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let ignore = false

    async function loadWorkItems() {
      try {
        const apiWorkItems = await getWorkItems()

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
        <WorkItemsList workItems={workItems} />
      )}
    </main>
  )
}

export default App

