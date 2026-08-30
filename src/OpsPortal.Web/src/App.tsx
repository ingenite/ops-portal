import { useEffect, useState } from "react";
import { type WorkItem } from "./models/WorkItem";
import { getWorkItems, createWorkItem } from "./api/workItemsApi";
import { WorkItemsList } from "./components/WorkItemsList";
import { CreateWorkItemForm } from "./components/CreateWorkItemForm";

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

  async function handleCreateWorkItem(title: string) {
    try {
      const createdWorkItem = await createWorkItem({ title })

      setWorkItems(currentWorkItems => [
        createdWorkItem,
        ...currentWorkItems
      ])
    } catch (error: unknown) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : 'An unknown error occurred'
      )
    }
  }

  return (
    <main>
      <h1>Work Items</h1>
      {isLoading ? (
        <p>Loading work items...</p>
      ) : errorMessage ? (
        <p role="alert">{errorMessage}</p>
      ) : (
        <>
          <section>
            <h2>Create New Work Item</h2>
            <CreateWorkItemForm onCreate={handleCreateWorkItem} />
          </section>

          {workItems.length === 0 ? (
          <p>No work items found.</p>
          ) : (
          <WorkItemsList workItems={workItems} />
          )}
        </>
      )}
    </main>
  )
}
  export default App
