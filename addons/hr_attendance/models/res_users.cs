csharp
public partial class User
{
    public void CleanAttendanceOfficers()
    {
        var attendanceOfficers = Env.Query<Hr.Employee>()
            .Where(e => e.AttendanceManagerId.In(this))
            .Select(e => e.AttendanceManagerId)
            .ToList();

        var officersToRemoveIds = new List<User> { this }.Except(attendanceOfficers);

        if (officersToRemoveIds.Any())
        {
            var groupHrAttendanceOfficer = Env.Ref<Res.Groups>("hr_attendance.group_hr_attendance_officer");
            groupHrAttendanceOfficer.Users = groupHrAttendanceOfficer.Users.Except(officersToRemoveIds).ToList();
        }
    }

    public Dictionary<string, object> ActionOpenLastMonthAttendances()
    {
        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["name"] = "Attendances This Month",
            ["res_model"] = "Hr.Attendance",
            ["views"] = new List<object>
            {
                new List<object>
                {
                    Env.Ref<Ir.UiView>("hr_attendance.hr_attendance_employee_simple_tree_view").Id,
                    "tree"
                }
            },
            ["context"] = new Dictionary<string, object>
            {
                ["create"] = 0
            },
            ["domain"] = new List<List<object>>
            {
                new List<object> { "EmployeeId", "=", this.EmployeeId.Id },
                new List<object> { "CheckIn", ">=", DateTime.Today.AddDays(1 - DateTime.Today.Day).Date }
            }
        };
    }

    public Dictionary<string, object> ActionOpenLastMonthOvertime()
    {
        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["name"] = "Overtime",
            ["res_model"] = "Hr.Attendance.Overtime",
            ["views"] = new List<object> { new List<object> { false, "tree" } },
            ["context"] = new Dictionary<string, object>
            {
                ["create"] = 0
            },
            ["domain"] = new List<List<object>>
            {
                new List<object> { "EmployeeId", "=", this.EmployeeId.Id }
            }
        };
    }
}
