import { cleanup, render, screen } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import { afterEach, describe, expect, it, vi } from "vitest"
import { WorkItemsList } from "./WorkItemsList"
import { createWorkItem } from "../test/factories/workItem"

afterEach(() => {
    cleanup()
})

describe("WorkItemsList", () => {
    it("calls onDelete with the work item's id when Delete is clicked", async () => {
      const user = userEvent.setup()

      const workItem = createWorkItem()

      const onDelete = vi.fn().mockResolvedValue(undefined)
      const onStatusChange = vi.fn().mockResolvedValue(undefined)

      render(
        <WorkItemsList
            workItems={[workItem]}
            onDelete={onDelete}
            onStatusChange={onStatusChange}
        />
      )

      await user.click(
        screen.getByRole("button", { name: `Delete ${workItem.title}` })
      )

      expect(onDelete).toHaveBeenCalledWith(workItem.id)
      expect(onDelete).toHaveBeenCalledTimes(1)
    })

    it("calls onStatusChange with the work item's id and selected status", async () => {
        const user = userEvent.setup()

        const workItem = createWorkItem()

        const onDelete = vi.fn().mockResolvedValue(undefined)
        const onStatusChange = vi.fn().mockResolvedValue(undefined)

        render(
            <WorkItemsList
                workItems={[workItem]}
                onDelete={onDelete}
                onStatusChange={onStatusChange}
            />
        )

        await user.selectOptions(
            screen.getByRole("combobox", { name: `Status for ${workItem.title}`,
            }),
            "Completed"
        )

        expect(onStatusChange).toHaveBeenCalledWith(workItem.id, "Completed")
        expect(onStatusChange).toHaveBeenCalledTimes(1)
    })
})