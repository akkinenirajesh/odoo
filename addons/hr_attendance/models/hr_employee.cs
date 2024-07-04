csharp
public partial class Employee
{
    public Hr.Attendance AttendanceActionChange(Dictionary<string, object> geoInformation = null)
    {
        var actionDate = DateTime.Now;

        if (AttendanceState != Hr.AttendanceStatus.CheckedIn)
        {
            var vals = new Dictionary<string, object>
            {
                { "EmployeeId", this.Id },
                { "CheckIn", actionDate }
            };

            if (geoInformation != null)
            {
                foreach (var key in geoInformation.Keys)
                {
                    vals[$"In{key}"] = geoInformation[key];
                }
            }

            return Env.Create<Hr.Attendance>(vals);
        }

        var attendance = Env.Query<Hr.Attendance>()
            .Where(a => a.EmployeeId == this.Id && a.CheckOut == null)
            .FirstOrDefault();

        if (attendance != null)
        {
            var updateVals = new Dictionary<string, object>
            {
                { "CheckOut", actionDate }
            };

            if (geoInformation != null)
            {
                foreach (var key in geoInformation.Keys)
                {
                    updateVals[$"Out{key}"] = geoInformation[key];
                }
            }

            attendance.Write(updateVals);
        }
        else
        {
            throw new UserException($"Cannot perform check out on {this.Name}, could not find corresponding check in. Your attendances have probably been modified manually by human resources.");
        }

        return attendance;
    }

    public ActionResult ActionOpenLastMonthAttendances()
    {
        return new ActionResult
        {
            Type = ActionType.Window,
            Name = "Attendances This Month",
            Model = "Hr.Attendance",
            ViewMode = "tree",
            ViewId = Env.Ref("Hr.Attendance.HrAttendanceEmployeeSimpleTreeView").Id,
            Context = new Dictionary<string, object> { { "create", false } },
            Domain = new List<object>
            {
                new List<object> { "EmployeeId", "=", this.Id },
                new List<object> { "CheckIn", ">=", DateTime.Today.AddDays(1 - DateTime.Today.Day).Date }
            }
        };
    }

    public ActionResult ActionOpenLastMonthOvertime()
    {
        return new ActionResult
        {
            Type = ActionType.Window,
            Name = "Overtime",
            Model = "Hr.Attendance.Overtime",
            ViewMode = "tree",
            Context = new Dictionary<string, object> { { "create", false } },
            Domain = new List<object> { new List<object> { "EmployeeId", "=", this.Id } }
        };
    }
}
