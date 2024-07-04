csharp
public partial class TimesheetAttendance
{
    public override string ToString()
    {
        return $"{Employee.Name} - {Date:d}";
    }

    public void Init()
    {
        Env.Cr.Execute("DROP VIEW IF EXISTS hr_timesheet_attendance_report");
        Env.Cr.Execute(@"
            CREATE OR REPLACE VIEW hr_timesheet_attendance_report AS (
                SELECT
                    max(id) AS id,
                    t.employee_id,
                    t.date,
                    t.company_id,
                    coalesce(sum(t.attendance), 0) AS total_attendance,
                    coalesce(sum(t.timesheet), 0) AS total_timesheet,
                    coalesce(sum(t.attendance), 0) - coalesce(sum(t.timesheet), 0) as total_difference,
                    NULLIF(sum(t.timesheet) * t.emp_cost, 0) as timesheets_cost,
                    NULLIF(sum(t.attendance) * t.emp_cost, 0) as attendance_cost,
                    NULLIF((coalesce(sum(t.attendance), 0) -  coalesce(sum(t.timesheet), 0)) * t.emp_cost, 0)  as cost_difference
                FROM (
                    SELECT
                        -hr_attendance.id AS id,
                        hr_employee.hourly_cost AS emp_cost,
                        hr_attendance.employee_id AS employee_id,
                        hr_attendance.worked_hours AS attendance,
                        NULL AS timesheet,
                        hr_attendance.check_in::date AS date,
                        hr_employee.company_id as company_id
                    FROM hr_attendance
                    LEFT JOIN hr_employee ON hr_employee.id = hr_attendance.employee_id
                UNION ALL
                    SELECT
                        ts.id AS id,
                        hr_employee.hourly_cost AS emp_cost,
                        ts.employee_id AS employee_id,
                        NULL AS attendance,
                        ts.unit_amount AS timesheet,
                        ts.date AS date,
                        ts.company_id AS company_id
                    FROM account_analytic_line AS ts
                    LEFT JOIN hr_employee ON hr_employee.id = ts.employee_id
                    WHERE ts.project_id IS NOT NULL
                ) AS t
                GROUP BY t.employee_id, t.date, t.company_id, t.emp_cost
                ORDER BY t.date
            )
        ");
    }

    public IEnumerable<IGrouping<object, TimesheetAttendance>> ReadGroup(
        Expression<Func<TimesheetAttendance, bool>> domain,
        IEnumerable<string> fields,
        IEnumerable<string> groupBy,
        int offset = 0,
        int? limit = null,
        string orderBy = null,
        bool lazy = true)
    {
        if (string.IsNullOrEmpty(orderBy) && groupBy.Any())
        {
            var orderByList = groupBy.Select(g => g.Split(':')[0]);
            orderBy = string.Join(",", orderByList.Select(field => field == "Date" ? $"{field} desc" : field));
        }

        return Env.Set<TimesheetAttendance>().ReadGroup(domain, fields, groupBy, offset, limit, orderBy, lazy);
    }
}
