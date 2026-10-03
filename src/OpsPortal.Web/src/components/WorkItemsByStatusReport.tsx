import type { WorkItemsByStatusReportRow } from "../models/WorkItemsByStatusReportRow";

type WorkItemsByStatusReportProps = {
    rows: WorkItemsByStatusReportRow[]
}

export function WorkItemsByStatusReport({
    rows
}: WorkItemsByStatusReportProps) {

    return (
        <div className="table-container">
            <table className="data-table">
                <caption>Work Items Status Report</caption>

                <thead>
                    <tr>
                        <th scope="col">Status</th>
                        <th scope="col" className="numeric-cell">Count</th>
                    </tr>
                </thead>
                <tbody>
                    {rows.map((row) => (
                        <tr key={row.status}>
                            <td>{row.status}</td>
                            <td className="numeric-cell">{row.count}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}
