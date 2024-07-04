csharp
public partial class LeaveType
{
    public string ComputeDisplayName()
    {
        if (!RequestedDisplayName() || Env.Context.Get<string>("RequestType", "leave") == "allocation")
        {
            return base.ComputeDisplayName();
        }

        var employee = Env.Get<HumanResources.Employee>().Browse(Env.Context.Get<int>("EmployeeId")).Sudo();
        if (employee.TotalOvertime <= 0)
        {
            return base.ComputeDisplayName();
        }

        if (OvertimeDeductible && RequiresAllocation == "no")
        {
            return $"{Name} ({FormatDuration(employee.TotalOvertime)} hours available)";
        }

        return base.ComputeDisplayName();
    }

    public Dictionary<HumanResources.Employee, List<Tuple<string, Dictionary<string, object>>>> GetAllocationData(List<HumanResources.Employee> employees, DateTime? date = null)
    {
        var res = base.GetAllocationData(employees, date);
        var deductibleTimeOffTypes = Env.Get<HumanResources.LeaveType>().Search(new[]
        {
            ("OvertimeDeductible", "=", true),
            ("RequiresAllocation", "=", "no")
        });
        var leaveTypeNames = deductibleTimeOffTypes.Select(lt => lt.Name).ToList();

        foreach (var employee in res.Keys)
        {
            foreach (var leaveData in res[employee])
            {
                if (leaveTypeNames.Contains(leaveData.Item1))
                {
                    leaveData.Item2["VirtualRemainingLeaves"] = employee.Sudo().TotalOvertime;
                    leaveData.Item2["OvertimeDeductible"] = true;
                }
                else
                {
                    leaveData.Item2["OvertimeDeductible"] = false;
                }
            }
        }

        return res;
    }

    public Tuple<Dictionary<string, object>, Dictionary<string, object>> GetDaysRequest(DateTime? date = null)
    {
        var res = base.GetDaysRequest(date);
        res.Item2["OvertimeDeductible"] = OvertimeDeductible;
        return res;
    }

    public void ComputeHrAttendanceOvertime()
    {
        if (Company != null)
        {
            HrAttendanceOvertime = Company.HrAttendanceOvertime;
        }
        else
        {
            HrAttendanceOvertime = Env.Company.HrAttendanceOvertime;
        }
    }

    private string FormatDuration(double hours)
    {
        // Implement the logic to format duration
        return hours.ToString("F2");
    }

    private bool RequestedDisplayName()
    {
        // Implement the logic to check if display name is requested
        return true;
    }
}
