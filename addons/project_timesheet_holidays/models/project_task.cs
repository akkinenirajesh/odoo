csharp
public partial class ProjectTask 
{
    public int LeaveTypesCount { get; set; }
    public bool IsTimeOffTask { get; set; }

    public void ComputeLeaveTypesCount()
    {
        var timeOffTypeReadGroup = Env.Get("hr.leave.type").ReadGroup(
            new[] { new("timesheet_task_id", "in", this.Id.ToString()) },
            new[] { "timesheet_task_id" },
            new[] { "__count" }
        );
        var timeOffTypeCountPerTask = timeOffTypeReadGroup.Select(r => (r.Id, r.Count)).ToDictionary(x => x.Id, x => x.Count);
        LeaveTypesCount = timeOffTypeCountPerTask.GetValueOrDefault(this.Id, 0);
    }

    public void ComputeIsTimeOffTask()
    {
        var timeoffTasks = this.Where(t => t.LeaveTypesCount > 0 || t.CompanyId.LeaveTimesheetTaskId == t.Id);
        timeoffTasks.ForEach(t => t.IsTimeOffTask = true);
        this.Except(timeoffTasks).ForEach(t => t.IsTimeOffTask = false);
    }

    public void SearchIsTimeOffTask(string operator, bool value)
    {
        if (operator != "=" && operator != "!=" || !value.GetType().Equals(typeof(bool)))
        {
            throw new NotImplementedException("Operation not supported");
        }
        var leaveTypeReadGroup = Env.Get("hr.leave.type").ReadGroup(
            new[] { new("timesheet_task_id", "!=", null) },
            new string[] { },
            new[] { "timesheet_task_id:recordset" }
        );
        var timeoffTasks = leaveTypeReadGroup[0].Recordset;
        if (Env.Company.LeaveTimesheetTaskId != null)
        {
            timeoffTasks = timeoffTasks.Union(Env.Company.LeaveTimesheetTaskId);
        }
        if (operator == "!=")
        {
            value = !value;
        }
        if (value)
        {
            this.Where(t => timeoffTasks.Contains(t)).ToList();
        }
        else
        {
            this.Where(t => !timeoffTasks.Contains(t)).ToList();
        }
    }
}
