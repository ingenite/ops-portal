import { cleanup, render, screen } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import { afterEach, describe, expect, it, vi } from "vitest"
import { CreateWorkItemForm } from "./CreateWorkItemForm"

afterEach(() => {
    cleanup()
})

describe("CreateWorkItemForm", () => {
    it("renders a title input and a Create button", () => {
        render(<CreateWorkItemForm onCreate={async () => {}} />)

        expect(
            screen.getByRole("textbox", { name: "New WorkItem Title:" })
        ).toBeInstanceOf(HTMLInputElement)

        expect(
            screen.getByRole("button", { name: "Create" })
        ).toBeInstanceOf(HTMLButtonElement)
    })

    it("calls onCreate with a trimmed title on submit", async () => {
        const user = userEvent.setup()
        const onCreate = vi.fn().mockResolvedValue(undefined)

        render(<CreateWorkItemForm onCreate={onCreate} />)

        await user.type(
            screen.getByRole("textbox", { name: "New WorkItem Title:" }),
            "  Review API contract  "
        )

        await user.click(
            screen.getByRole("button", { name: "Create" })
        )

        expect(onCreate).toHaveBeenCalledTimes(1)
        expect(onCreate).toHaveBeenCalledWith("Review API contract")
    })
})