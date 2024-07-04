csharp
public partial class ProjectTimesheetHolidays.ResourceCalendarLeaves {
    public ProjectTimesheetHolidays.ResourceCalendarLeaves(BuviContext context) {
        // Constructor
    }

    public void GetResourceCalendars() {
        // Implementation for _get_resource_calendars()
    }

    public Dictionary<DateTime, double> WorkTimePerDay(List<Resource.Calendar> resourceCalendars = null) {
        // Implementation for _work_time_per_day()
    }

    public void TimesheetCreateLines() {
        // Implementation for _timesheet_create_lines()
    }

    public Account.AnalyticLine TimesheetPrepareLineValues(int index, Hr.Employee employeeId, List<Tuple<DateTime, double>> workHoursData, DateTime dayDate, double workHoursCount) {
        // Implementation for _timesheet_prepare_line_values()
    }

    public void GenerateTimesheeets() {
        // Implementation for _generate_timesheeets()
    }

    public void GeneratePublicTimeOffTimesheets(List<Hr.Employee> employees) {
        // Implementation for _generate_public_time_off_timesheets()
    }

    public void Write(Dictionary<string, object> vals) {
        // Implementation for write()
    }
}
