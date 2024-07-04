csharp
public partial class ProjectTimesheetHolidays.HolidaysType
{
    public bool TimesheetGenerate { get; set; }
    public Project.Project TimesheetProject { get; set; }
    public Project.Task TimesheetTask { get; set; }

    public void ComputeTimesheetGenerate()
    {
        this.TimesheetGenerate = !Env.Company.IsNull() && this.TimesheetTask != null && this.TimesheetProject != null;
    }

    public void ComputeTimesheetProject()
    {
        this.TimesheetProject = Env.Company.InternalProject;
    }

    public void ComputeTimesheetTask()
    {
        var defaultTaskId = Env.Company.LeaveTimesheetTask;
        if (defaultTaskId != null && defaultTaskId.Project == this.TimesheetProject)
        {
            this.TimesheetTask = defaultTaskId;
        }
        else
        {
            this.TimesheetTask = null;
        }
    }

    public void CheckTimesheetGenerate()
    {
        if (this.TimesheetGenerate && !Env.Company.IsNull())
        {
            if (this.TimesheetProject == null || this.TimesheetTask == null)
            {
                throw new Exception("Both the internal project and task are required to generate a timesheet for the time off " + this.Name + ". If you don't want a timesheet, you should leave the internal project and task empty.");
            }
        }
    }
}

public partial class ProjectTimesheetHolidays.Holidays
{
    public Account.AnalyticLine[] TimesheetIds { get; set; }

    public void ValidateLeaveRequest()
    {
        List<Account.AnalyticLine> valsList = new List<Account.AnalyticLine>();
        List<int> leaveIds = new List<int>();
        foreach (var leave in this)
        {
            if (!leave.HolidayStatus.TimesheetGenerate)
            {
                continue;
            }

            Project.Project project;
            Project.Task task;

            if (!leave.HolidayStatus.Company.IsNull())
            {
                project = leave.HolidayStatus.TimesheetProject;
                task = leave.HolidayStatus.TimesheetTask;
            }
            else
            {
                project = leave.Employee.Company.InternalProject;
                task = leave.Employee.Company.LeaveTimesheetTask;
            }

            if (project == null || task == null)
            {
                continue;
            }

            leaveIds.Add(leave.Id);

            if (leave.Employee == null)
            {
                continue;
            }

            var workHoursData = leave.Employee.ListWorkTimePerDay(leave.DateFrom, leave.DateTo)[leave.Employee.Id];

            for (int index = 0; index < workHoursData.Count; index++)
            {
                var (dayDate, workHoursCount) = workHoursData[index];
                valsList.Add(PrepareTimesheetLineValues(index, workHoursData, dayDate, workHoursCount, project, task));
            }
        }

        // Unlink previous timesheets to avoid doublon (shouldn't happen on the interface but meh)
        var oldTimesheets = Env.AnalyticLine.Search([('Project', '!=', null), ('Holiday', 'in', leaveIds)]);
        if (oldTimesheets != null)
        {
            oldTimesheets.Holiday = null;
            oldTimesheets.Unlink();
        }

        Env.AnalyticLine.Create(valsList.ToArray());

        base.ValidateLeaveRequest();
    }

    public Account.AnalyticLine PrepareTimesheetLineValues(int index, List<(DateTime, double)> workHoursData, DateTime dayDate, double workHoursCount, Project.Project project, Project.Task task)
    {
        return new Account.AnalyticLine
        {
            Name = "Time Off (" + (index + 1) + "/" + workHoursData.Count + ")",
            Project = project,
            Task = task,
            Account = project.AnalyticAccount,
            UnitAmount = workHoursCount,
            User = this.Employee.User,
            Date = dayDate,
            Holiday = this,
            Employee = this.Employee,
            Company = task.Company ?? project.Company
        };
    }

    public void CheckMissingGlobalLeaveTimesheets()
    {
        if (this.Count == 0)
        {
            return;
        }

        var minDate = this.Min(x => x.DateFrom);
        var maxDate = this.Max(x => x.DateTo);

        var globalLeaves = Env.ResourceCalendarLeaves.Search([
            ("Resource", "=", null),
            ("DateTo", ">=", minDate),
            ("DateFrom", "<=", maxDate),
            ("Company.InternalProject", "!=", null),
            ("Company.LeaveTimesheetTask", "!=", null)
        ]);

        if (globalLeaves != null)
        {
            globalLeaves.GeneratePublicTimeOffTimesheets(this.Employee);
        }
    }

    public void ActionRefuse()
    {
        var result = base.ActionRefuse();

        var timesheets = this.TimesheetIds;
        timesheets.Holiday = null;
        timesheets.Unlink();

        CheckMissingGlobalLeaveTimesheets();

        return result;
    }

    public void ActionUserCancel(string reason)
    {
        var res = base.ActionUserCancel(reason);

        var timesheets = this.TimesheetIds;
        timesheets.Holiday = null;
        timesheets.Unlink();

        CheckMissingGlobalLeaveTimesheets();

        return res;
    }
}
