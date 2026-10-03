import { cleanup, render, screen, within } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { getWorkItemsByStatusReport } from "../api/reportsApi";
import { WorkItemsByStatusReportView } from "./WorkItemsByStatusReportView";

afterEach(() => {
  cleanup();
});

vi.mock("../api/reportsApi", () => ({
  getWorkItemsByStatusReport: vi.fn(),
}));

describe("WorkItemsByStatusReportView", () => {
  it("loads and displays status counts, including zero", async () => {
    vi.mocked(getWorkItemsByStatusReport).mockResolvedValue([
      { status: "Open", count: 2 },
      { status: "InProgress", count: 3 },
      { status: "Completed", count: 0 },
    ]);

    render(<WorkItemsByStatusReportView />);

    expect(screen.getByRole("status").textContent).toBe(
      "Loading Status Report",
    );

    const table = await screen.findByRole("table", {
      name: "Work Items Status Report",
    });

    expect(within(table).getByRole("row", { name: "Open 2" })).toBeTruthy();

    expect(
      within(table).getByRole("row", { name: "InProgress 3" }),
    ).toBeTruthy();

    expect(
      within(table).getByRole("row", { name: "Completed 0" }),
    ).toBeTruthy();

    expect(screen.queryByRole("status")).toBeNull();
  });
});
