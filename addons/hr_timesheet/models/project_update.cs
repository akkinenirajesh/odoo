csharp
public partial class ProjectUpdate
{
    public bool ComputeDisplayTimesheetStats()
    {
        return ProjectId.AllowTimesheets;
    }

    public int ComputeTimesheetPercentage()
    {
        return AllocatedTime != 0 ? (int)Math.Round(TimesheetTime * 100.0 / AllocatedTime) : 0;
    }

    public static List<ProjectUpdate> Create(List<Dictionary<string, object>> valsList)
    {
        var updates = base.Create(valsList);
        var encodeUom = Env.Company.TimesheetEncodeUomId;
        var ratio = Env.Ref("uom.product_uom_hour").Ratio / encodeUom.Ratio;

        foreach (var update in updates)
        {
            var project = update.ProjectId;
            project.WithSudo().LastUpdateId = update;
            update.Write(new Dictionary<string, object>
            {
                { "UomId", encodeUom },
                { "AllocatedTime", (int)Math.Round(project.AllocatedHours / ratio) },
                { "TimesheetTime", (int)Math.Round(project.TotalTimesheetTime / ratio) }
            });
        }

        return updates;
    }
}
