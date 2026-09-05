import type { WorkItemsByStatusReportRow } from "../models/WorkItemsByStatusReportRow";

type WorkItemsByStatusReportProps = {
    rows: WorkItemsByStatusReportRow[]
}

export function WorkItemsByStatusReport({
    rows
}: WorkItemsByStatusReportProps) {

    return (
        <table>
            <caption>Work Items Status Report</caption>

            <thead>
                <tr>
                    <th scope="col">Status</th>
                    <th scope="col">Count</th>
                </tr>
            </thead>
            <tbody>
                {rows.map((row) => (
                    <tr key={row.status}>
                        <td>{row.status}</td>
                        <td>{row.count}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    )
}
