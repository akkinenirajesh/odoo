csharp
public partial class AccountAnalyticLine
{
    public override string ToString()
    {
        if (ProjectId != null)
        {
            if (TaskId != null)
            {
                return $"{ProjectId.DisplayName} - {TaskId.DisplayName}";
            }
            return ProjectId.DisplayName;
        }
        return base.ToString();
    }

    private bool IsReadonly()
    {
        // is overridden in other timesheet related modules
        return false;
    }

    public void ComputeReadonlyTimesheet()
    {
        if (!Env.User.HasGroup("base.group_user"))
        {
            ReadonlyTimesheet = true;
        }
        else
        {
            ReadonlyTimesheet = IsReadonly();
        }
    }

    public void ComputeEncodingUomId()
    {
        EncodingUomId = CompanyId.TimesheetEncodeUomId;
    }

    public void ComputePartnerId()
    {
        if (ProjectId != null)
        {
            PartnerId = TaskId?.PartnerId ?? ProjectId.PartnerId;
        }
    }

    public void ComputeProjectId()
    {
        if (TaskId?.ProjectId != null && ProjectId != TaskId.ProjectId)
        {
            ProjectId = TaskId.ProjectId;
        }
    }

    public void ComputeTaskId()
    {
        if (ProjectId == null)
        {
            TaskId = null;
        }
    }

    public void OnChangeProjectId()
    {
        if (ProjectId != TaskId?.ProjectId)
        {
            TaskId = null;
        }
    }

    public void ComputeUserId()
    {
        UserId = EmployeeId?.UserId ?? DefaultUser();
    }

    public void ComputeDepartmentId()
    {
        DepartmentId = EmployeeId?.DepartmentId;
    }

    private Core.User DefaultUser()
    {
        return Env.Context.GetValueOrDefault("user_id", Env.User) as Core.User;
    }

    // Add other methods as needed...
}
