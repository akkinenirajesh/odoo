csharp
public partial class HolidaysAllocation
{
    public object DefaultGet(string[] fields)
    {
        var res = base.DefaultGet(fields);
        if (fields.Contains("HolidayStatusId") && Env.Context.GetValueOrDefault("DeductExtraHours", false))
        {
            var domain = new List<object[]>
            {
                new object[] { "OvertimeDeductible", "=", true },
                new object[] { "RequiresAllocation", "=", "yes" }
            };
            if (Env.Context.GetValueOrDefault("DeductExtraHoursEmployeeRequest", false))
            {
                domain.Add(new object[] { "EmployeeRequests", "=", "yes" });
            }
            var leaveType = Env.Set<HumanResources.LeaveType>().Search(domain).FirstOrDefault();
            res["HolidayStatusId"] = leaveType?.Id;
        }
        return res;
    }

    public void ComputeOvertimeDeductible()
    {
        OvertimeDeductible = HrAttendanceOvertime && HolidayStatusId.OvertimeDeductible;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (OvertimeDeductible)
        {
            var duration = NumberOfHoursDisplay;
            if (duration > Employee.TotalOvertime)
            {
                throw new ValidationException("The employee does not have enough overtime hours to request this leave.");
            }
            if (OvertimeId == null)
            {
                OvertimeId = Env.Set<HumanResources.AttendanceOvertime>().Create(new Dictionary<string, object>
                {
                    { "Employee", Employee.Id },
                    { "Date", DateFrom },
                    { "Adjustment", true },
                    { "Duration", -1 * duration }
                });
            }
        }
    }

    public override void OnWrite(Dictionary<string, object> vals)
    {
        base.OnWrite(vals);
        if (!vals.ContainsKey("NumberOfDays")) return;

        if (OvertimeId != null)
        {
            var duration = NumberOfHoursDisplay;
            var overtimeDuration = OvertimeId.Duration;
            if (overtimeDuration != -1 * duration)
            {
                if (duration > Employee.TotalOvertime - overtimeDuration)
                {
                    throw new ValidationException("The employee does not have enough extra hours to extend this allocation.");
                }
                OvertimeId.Duration = -1 * duration;
            }
        }
    }

    public void ActionRefuse()
    {
        base.ActionRefuse();
        OvertimeId?.Delete();
    }

    public float GetAccrualPlanLevelWorkEntryProrata(AccrualPlanLevel level, DateTime startPeriod, DateTime startDate, DateTime endPeriod, DateTime endDate)
    {
        if (level.Frequency != "hourly" || level.FrequencyHourlySource != "attendance")
        {
            return base.GetAccrualPlanLevelWorkEntryProrata(level, startPeriod, startDate, endPeriod, endDate);
        }

        var startDt = startDate.Date;
        var endDt = endDate.Date;
        var attendances = Env.Set<HumanResources.Attendance>().Search(new List<object[]>
        {
            new object[] { "Employee", "=", Employee.Id },
            new object[] { "CheckIn", ">=", startDt },
            new object[] { "CheckOut", "<=", endDt }
        });

        return attendances.Sum(a => a.WorkedHours);
    }
}
