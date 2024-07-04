csharp
public partial class HolidaysType
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeLeaves()
    {
        var employee = Env.HrEmployee.GetContextualEmployee();
        var targetDate = Env.Context.GetValueOrDefault("DefaultDateFrom", DateTime.Today);
        var data = GetAllocationData(employee, targetDate)[employee];
        var result = data.FirstOrDefault(item => item.Item1 == Name);
        if (result != null)
        {
            MaxLeaves = result.Item2.GetValueOrDefault("MaxLeaves", 0);
            LeavesTaken = result.Item2.GetValueOrDefault("LeavesTaken", 0);
            VirtualRemainingLeaves = result.Item2.GetValueOrDefault("VirtualRemainingLeaves", 0);
        }
        else
        {
            MaxLeaves = 0;
            LeavesTaken = 0;
            VirtualRemainingLeaves = 0;
        }
    }

    public void ComputeAllocationCount()
    {
        var minDateTime = new DateTime(DateTime.Now.Year, 1, 1);
        var maxDateTime = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        var domain = new List<object>
        {
            new List<object> { "HolidayStatusId", "=", Id },
            new List<object> { "DateFrom", ">=", minDateTime },
            new List<object> { "DateFrom", "<=", maxDateTime },
            new List<object> { "State", "in", new List<string> { "confirm", "validate" } }
        };

        AllocationCount = Env.HrLeaveAllocation.Search(domain).Count();
    }

    public void ComputeGroupDaysLeave()
    {
        var minDateTime = new DateTime(DateTime.Now.Year, 1, 1);
        var maxDateTime = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        var domain = new List<object>
        {
            new List<object> { "HolidayStatusId", "=", Id },
            new List<object> { "DateFrom", ">=", minDateTime },
            new List<object> { "DateFrom", "<=", maxDateTime },
            new List<object> { "State", "in", new List<string> { "validate", "validate1", "confirm" } }
        };

        GroupDaysLeave = Env.HrLeave.Search(domain).Count();
    }

    public void ComputeAccrualCount()
    {
        AccrualCount = Env.HrLeaveAccrualPlan.Search(new List<object> { new List<object> { "TimeOffTypeId", "=", Id } }).Count();
    }

    public void ComputeValid()
    {
        var dateFrom = Env.Context.GetValueOrDefault("DefaultDateFrom", DateTime.Today);
        var dateTo = Env.Context.GetValueOrDefault("DefaultDateTo", DateTime.Today);
        var employeeId = Env.Context.GetValueOrDefault("DefaultEmployeeId", Env.Context.GetValueOrDefault("EmployeeId", Env.User.EmployeeId));

        if (RequiresAllocation == RequiresAllocation.Yes)
        {
            var allocations = Env.HrLeaveAllocation.Search(new List<object>
            {
                new List<object> { "HolidayStatusId", "=", Id },
                new List<object> { "AllocationType", "=", "accrual" },
                new List<object> { "EmployeeId", "=", employeeId },
                new List<object> { "DateFrom", "<=", dateFrom },
                "|",
                new List<object> { "DateTo", ">=", dateTo },
                new List<object> { "DateTo", "=", false }
            });

            var allowedExcess = AllowsNegative ? MaxAllowedNegative : 0;
            allocations = allocations.Where(alloc =>
                alloc.AllocationType == "accrual"
                || (alloc.MaxLeaves > 0 && alloc.VirtualRemainingLeaves > -allowedExcess)
            );

            HasValidAllocation = allocations.Any();
        }
        else
        {
            HasValidAllocation = true;
        }
    }

    public List<object> ActionSeeDaysAllocated()
    {
        var action = Env.IrActionsActions.ForXmlId("hr_holidays", "hr_leave_allocation_action_all");
        action["domain"] = new List<object> { new List<object> { "HolidayStatusId", "in", new List<int> { Id } } };
        action["context"] = new Dictionary<string, object>
        {
            { "default_holiday_status_id", Id },
            { "search_default_approved_state", 1 },
            { "search_default_year", 1 }
        };
        return action;
    }

    public List<object> ActionSeeGroupLeaves()
    {
        var action = Env.IrActionsActions.ForXmlId("hr_holidays", "hr_leave_action_action_approve_department");
        action["domain"] = new List<object> { new List<object> { "HolidayStatusId", "=", Id } };
        action["context"] = new Dictionary<string, object>
        {
            { "default_holiday_status_id", Id }
        };
        return action;
    }

    public List<object> ActionSeeAccrualPlans()
    {
        var action = Env.IrActionsActions.ForXmlId("hr_holidays", "open_view_accrual_plans");
        action["domain"] = new List<object> { new List<object> { "TimeOffTypeId", "=", Id } };
        action["context"] = new Dictionary<string, object>
        {
            { "default_time_off_type_id", Id }
        };
        return action;
    }
}
