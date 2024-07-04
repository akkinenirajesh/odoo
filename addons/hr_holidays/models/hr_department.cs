csharp
public partial class Department
{
    public void ComputeLeaveCount()
    {
        var Requests = Env.Get<HR.Leave>();
        var Allocations = Env.Get<HR.LeaveAllocation>();
        var todayDate = DateTime.UtcNow.Date;
        var todayStart = todayDate.ToString("o");
        var todayEnd = todayDate.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("o");

        var leaveData = Requests.ReadGroup(
            new[] { ("DepartmentId", "=", Id), ("State", "=", "confirm") },
            new[] { "DepartmentId" },
            new[] { "__count" }
        );
        var allocationData = Allocations.ReadGroup(
            new[] { ("DepartmentId", "=", Id), ("State", "=", "confirm") },
            new[] { "DepartmentId" },
            new[] { "__count" }
        );
        var absenceData = Requests.ReadGroup(
            new[] {
                ("DepartmentId", "=", Id),
                ("State", "not in", new[] { "cancel", "refuse" }),
                ("DateFrom", "<=", todayEnd),
                ("DateTo", ">=", todayStart)
            },
            new[] { "DepartmentId" },
            new[] { "__count" }
        );

        LeaveToApproveCount = leaveData.FirstOrDefault()?.Count ?? 0;
        AllocationToApproveCount = allocationData.FirstOrDefault()?.Count ?? 0;
        AbsenceOfToday = absenceData.FirstOrDefault()?.Count ?? 0;
    }

    public Dictionary<string, object> GetActionContext()
    {
        return new Dictionary<string, object>
        {
            { "search_default_approve", 1 },
            { "search_default_active_employee", 2 },
            { "search_default_department_id", Id },
            { "default_department_id", Id },
            { "searchpanel_default_department_id", Id }
        };
    }

    public Dictionary<string, object> ActionOpenLeaveDepartment()
    {
        var action = Env.Get<IR.Actions>().ForXmlId("HR.Holidays", "hr_leave_action_action_approve_department");
        var context = GetActionContext();
        context["search_default_active_time_off"] = 3;
        context["hide_employee_name"] = 1;
        context["holiday_status_display_name"] = false;
        action["context"] = context;
        return action;
    }

    public Dictionary<string, object> ActionOpenAllocationDepartment()
    {
        var action = Env.Get<IR.Actions>().ForXmlId("HR.Holidays", "hr_leave_allocation_action_approve_department");
        var context = GetActionContext();
        context["search_default_second_approval"] = 3;
        action["context"] = context;
        action["domain"] = Expression.And(action["domain"].ToString(), new[] { ("State", "=", "confirm") });
        return action;
    }
}
