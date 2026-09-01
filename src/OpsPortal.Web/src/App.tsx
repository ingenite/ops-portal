import { useEffect, useState } from "react";
import type {  WorkItem, WorkItemStatus } from "./models/WorkItem";
import { getWorkItems, createWorkItem, deleteWorkItem, updateWorkItem } from "./api/workItemsApi";
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
    const createdWorkItem = await createWorkItem({ title })

    setWorkItems(currentWorkItems => [
      createdWorkItem,
      ...currentWorkItems
    ])
  }

  async function handleDeleteWorkItem(id: string) {
    await deleteWorkItem(id)

    setWorkItems(currentWorkItems =>
      currentWorkItems.filter(workItem => workItem.id !== id)
    )
  }

  async function handleStatusChange(
    id: string,
    status: WorkItemStatus
  ) {
    const updatedWorkItem = await updateWorkItem(id, { status} )

    setWorkItems(currentWorkItems =>
      currentWorkItems.map(workItem =>
        workItem.id === updatedWorkItem.id
          ? updatedWorkItem
          : workItem
      )
    )
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
          <WorkItemsList
            workItems={workItems}
            onDelete={handleDeleteWorkItem}
            onStatusChange={handleStatusChange}
          />
          )}
        </>
      )}
    </main>
  )
}
  export default App
