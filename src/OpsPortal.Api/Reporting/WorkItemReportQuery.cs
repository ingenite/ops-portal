using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Contracts;
using OpsPortal.Api.Data;

namespace OpsPortal.Api.Reporting;

public sealed class WorkItemReportQuery
{
    private readonly OpsPortalDbContext _dbContext;

    public WorkItemReportQuery(OpsPortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkItemsByStatusReportRow>> GetStatusCountsAsync()
    {
        var counts = await _dbContext.WorkItems
            .GroupBy(workItem => workItem.Status)
            .Select(group => new WorkItemsByStatusReportRow
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToArrayAsync();

        List<string> status = ["Open", "InProgress", "Completed"];

        var report = new List<WorkItemsByStatusReportRow>();

        foreach (var item in status)
        {
            var row = counts.FirstOrDefault(row => row.Status == item);

            if (row is not null)
            {
                report.Add(row);
            }
            else
            {
                report.Add(
                    new WorkItemsByStatusReportRow
                    {
                        Status = item,
                        Count = 0
                    });
            }
        }

        return report;
    }
}