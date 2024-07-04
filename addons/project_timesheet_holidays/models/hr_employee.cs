csharp
public partial class ProjectTimesheetHolidaysEmployee {
    public ProjectTimesheetHolidaysEmployee Create(Dictionary<string, object> valsList) {
        var employees = Env.Model("Project.TimesheetHolidays.Employee").Create(valsList);
        if (Env.Context.ContainsKey("salary_simulation")) {
            return employees;
        }
        Env.Context["allowed_company_ids"] = employees.GetValues("Company.Id");
        this._CreateFuturePublicHolidaysTimesheets(employees);
        return employees;
    }

    public void Write(Dictionary<string, object> vals) {
        var result = Env.Model("Project.TimesheetHolidays.Employee").Write(this, vals);
        var selfCompany = this.WithContext(new Dictionary<string, object> { { "allowed_company_ids", this.GetValues("Company.Id") } });
        if (vals.ContainsKey("Active")) {
            if ((bool)vals["Active"]) {
                var inactiveEmp = selfCompany.Where(e => !(bool)e.GetValues("Active"));
                inactiveEmp._CreateFuturePublicHolidaysTimesheets(this);
            } else {
                selfCompany._DeleteFuturePublicHolidaysTimesheets();
            }
        } else if (vals.ContainsKey("ResourceCalendar.Id")) {
            selfCompany._DeleteFuturePublicHolidaysTimesheets();
            selfCompany._CreateFuturePublicHolidaysTimesheets(this);
        }
    }

    public void _DeleteFuturePublicHolidaysTimesheets() {
        var futureTimesheets = Env.Model("Account.Analytic.Line").Search(new Dictionary<string, object> {
            { "GlobalLeave.Id", "!=", false },
            { "Date", ">=", Env.Datetime.Now().Date }
        }, new Dictionary<string, object> { { "employee_id", this.GetValues("Id") } });
        futureTimesheets.Write(new Dictionary<string, object> { { "GlobalLeave.Id", false } });
        futureTimesheets.Unlink();
    }

    public void _CreateFuturePublicHolidaysTimesheets(ProjectTimesheetHolidaysEmployee employees) {
        var linesVals = new List<Dictionary<string, object>>();
        var today = Env.Datetime.Now().Date;
        var globalLeavesWoCalendar = new Dictionary<int, object>();
        globalLeavesWoCalendar = (Dictionary<int, object>)Env.Model("Resource.Calendar.Leaves").ReadGroup(
            new Dictionary<string, object> { { "Calendar.Id", false }, { "DateFrom", ">=", today } },
            new List<string> { "Company.Id" }, new List<string> { "Id:recordset" });
        foreach (var employee in employees) {
            if (!(bool)employee.GetValues("Active")) {
                continue;
            }
            var globalLeaves = employee.GetValues("ResourceCalendar.GlobalLeave.Ids").Where(l => (DateTime)l.GetValues("DateFrom") >= today).ToList();
            globalLeaves.AddRange(((List<object>)globalLeavesWoCalendar[employee.GetValues("Company.Id")]).Cast<ResourceCalendarLeaves>().Where(l => (DateTime)l.GetValues("DateFrom") >= today));
            var workHoursData = globalLeaves.WorkTimePerDay();
            foreach (var globalTimeOff in globalLeaves) {
                var index = 0;
                foreach (var dayDateWorkHoursCount in workHoursData[employee.GetValues("ResourceCalendar.Id")][globalLeaves.First().GetValues("Id")]) {
                    var dayDate = (DateTime)dayDateWorkHoursCount[0];
                    var workHoursCount = (double)dayDateWorkHoursCount[1];
                    linesVals.Add(globalLeaves.First().TimesheetPrepareLineValues(index, employee, workHoursData[globalLeaves.First().GetValues("Id")], dayDate, workHoursCount));
                    index++;
                }
            }
        }
        Env.Model("Account.Analytic.Line").Create(linesVals);
    }
}
