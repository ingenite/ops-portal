import { useEffect, useState } from "react";
import { WorkItemsByStatusReport } from "./WorkItemsByStatusReport";
import { getWorkItemsByStatusReport } from "../api/reportsApi";
import type { WorkItemsByStatusReportRow } from "../models/WorkItemsByStatusReportRow";

export function WorkItemsByStatusReportView() {
    const [rows, setRows] = useState<WorkItemsByStatusReportRow[]>([])
    const [isLoading, setIsLoading] = useState(true)
    const [errorMessage, setErrorMessage] = useState<string | null>(null)

    useEffect(() => {
        let ignore = false

        async function loadWorkItemsByStatusReport() {
            try {
                const apiWorkItemsByStatusReport = await getWorkItemsByStatusReport()

                if (!ignore) {
                    setRows(apiWorkItemsByStatusReport)
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

        void loadWorkItemsByStatusReport()

        return () => {
            ignore = true
        }
    }, [])

    return (
        <>
        {isLoading ? (
            <p role="status">Loading Status Report</p>
        ) : errorMessage ? (
            <p role="alert">{errorMessage}</p>
        ) : (
            <section>
                <h2>Work Items Status Report</h2>
                <WorkItemsByStatusReport rows={rows} />
            </section>
        )}
        </>
    )
}
