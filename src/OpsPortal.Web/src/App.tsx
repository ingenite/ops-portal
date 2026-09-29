import { useEffect, useState } from "react";
import type {  WorkItem, WorkItemStatus } from "./models/WorkItem";
import { getWorkItems, createWorkItem, deleteWorkItem, updateWorkItem } from "./api/workItemsApi";
import { WorkItemsList } from "./components/WorkItemsList";
import { CreateWorkItemForm } from "./components/CreateWorkItemForm";
import { WorkItemsByStatusReportView } from "./components/WorkItemsByStatusReportView";
import keycloak from "./auth/keycloak";

function App() {
  const [workItems, setWorkItems] = useState<WorkItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [activeView, setActiveView] = useState<"workItems" | "reporting">("workItems")
  const [signOutError, setSignOutError] = useState<string | null>(null)

  const username = keycloak.tokenParsed?.preferred_username ?? 'Unknown user'

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

    async function handleSignOut() {
      setSignOutError(null)

      try {
        await keycloak.logout({
          redirectUri: window.location.origin,
        })
      } catch {
        setSignOutError('Unable to sign out. Please try again.')
      }
    }

  return (
    <main>
      <h1>OpsPortal</h1>

      <p>Signed in as {username}</p>

      <button type="button" onClick={handleSignOut}>
        Sign out
      </button>

      {signOutError && <p role="alert">{signOutError}</p>}

      <nav aria-label="Main">
        <button
          type="button"
          onClick={() => setActiveView("workItems")}
          aria-pressed={activeView === "workItems"}
        >
          Work Items
        </button>
        <button
          type="button"
          onClick={() => setActiveView("reporting")}
          aria-pressed={activeView === "reporting"}
        >
          Reporting
        </button>
      </nav>

      {activeView === "workItems" ? (
        <>
          <h2>Work Items</h2>

          {isLoading ? (
            <p>Loading work items...</p>
          ) : errorMessage ? (
            <p role="alert">{errorMessage}</p>
          ) : (
            <>
              <section>
                <h3>Create New Work Item</h3>
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
        </>
      ) : (
        <WorkItemsByStatusReportView />
      )}
    </main>
  )
}
export default App
