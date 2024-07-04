csharp
public partial class LeaveReportCalendar
{
    public override string ToString()
    {
        return Name;
    }

    private void _ComputeIsManager()
    {
        IsManager = Env.User.HasGroup("hr_holidays.group_hr_holidays_user") || LeaveManager == Env.User;
    }

    public void ActionApprove()
    {
        Leave.ActionApprove(checkState: false);
    }

    public void ActionValidate()
    {
        Leave.ActionValidate();
    }

    public void ActionRefuse()
    {
        Leave.ActionRefuse();
    }

    public Dictionary<DateTime, bool> GetUnusualDays(DateTime dateFrom, DateTime? dateTo = null)
    {
        return Env.User.Employee.GetUnusualDays(dateFrom, dateTo);
    }
}
