csharp
public partial class ResUsers
{
    public bool RequestOvertime { get; set; }

    public List<string> SELF_READABLE_FIELDS
    {
        get
        {
            var baseFields = base.SELF_READABLE_FIELDS;
            baseFields.Add("RequestOvertime");
            return baseFields;
        }
    }

    public void ComputeRequestOvertime()
    {
        var isHolidayUser = Env.User.HasGroup("hr_holidays.group_hr_holidays_user");
        var timeOffTypes = Env.Set<HrLeaveType>().SearchCount(new[]
        {
            ("RequiresAllocation", "=", "yes"),
            ("EmployeeRequests", "=", "yes"),
            ("OvertimeDeductible", "=", true)
        });

        if (TotalOvertime >= 1)
        {
            RequestOvertime = isHolidayUser ? true : timeOffTypes > 0;
        }
        else
        {
            RequestOvertime = false;
        }
    }
}
