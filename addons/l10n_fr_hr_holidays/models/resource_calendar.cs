csharp
public partial class ResourceCalendar
{
    public bool WorksOnDate(DateTime date)
    {
        var workingDays = GetWorkingHours();
        var dayOfWeek = date.DayOfWeek.ToString().ToLower();

        if (TwoWeeksCalendar)
        {
            var weekType = Env.Get<ResourceCalendarAttendance>().GetWeekType(date).ToString();
            return workingDays[weekType][dayOfWeek];
        }
        return workingDays[""][dayOfWeek];
    }

    private Dictionary<string, Dictionary<string, bool>> GetWorkingHours()
    {
        var workingDays = new Dictionary<string, Dictionary<string, bool>>();

        foreach (var attendance in AttendanceIds)
        {
            var weekType = attendance.WeekType ?? "";
            if (!workingDays.ContainsKey(weekType))
            {
                workingDays[weekType] = new Dictionary<string, bool>();
            }
            workingDays[weekType][attendance.Dayofweek] = true;
        }

        return workingDays;
    }
}
