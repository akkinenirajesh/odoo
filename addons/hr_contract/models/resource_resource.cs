csharp
public partial class ResourceResource
{
    public Dictionary<int, Dictionary<ResourceCalendar, Intervals>> GetCalendarsValidityWithinPeriod(DateTime start, DateTime end, Company defaultCompany = null)
    {
        if (!start.Kind.Equals(DateTimeKind.Utc) || !end.Kind.Equals(DateTimeKind.Utc))
            throw new ArgumentException("Start and end times must be in UTC.");

        if (this == null)
            return base.GetCalendarsValidityWithinPeriod(start, end, defaultCompany);

        var calendarsWithinPeriodPerResource = new Dictionary<int, Dictionary<ResourceCalendar, Intervals>>();

        var employeeIdsWithActiveContracts = Env.HrContract.ReadGroup(
            domain: new List<object>
            {
                ("EmployeeId", "in", this.EmployeeId.Ids),
                "|", ("State", "=", "open"),
                "|", ("State", "=", "close"),
                     "&", ("State", "=", "draft"), ("KanbanState", "=", "done")
            },
            groupBy: new List<string> { "EmployeeId" }
        ).Select(group => (int)group[0]).ToHashSet();

        var resourceWithoutContract = this.Where(r =>
            r.EmployeeId == null ||
            !employeeIdsWithActiveContracts.Contains(r.EmployeeId.Id) ||
            !new[] { "employee", "student" }.Contains(r.EmployeeId.EmployeeType)
        );

        if (resourceWithoutContract.Any())
        {
            var baseResult = base.GetCalendarsValidityWithinPeriod(start, end, defaultCompany);
            foreach (var kvp in baseResult)
            {
                calendarsWithinPeriodPerResource[kvp.Key] = kvp.Value;
            }
        }

        var resourceWithContract = this.Except(resourceWithoutContract);
        if (!resourceWithContract.Any())
            return calendarsWithinPeriodPerResource;

        var timezones = resourceWithContract.Select(r => r.Tz).Distinct();
        var dateStart = timezones.Min(tz => start.ToZone(tz).Date);
        var dateEnd = timezones.Max(tz => end.ToZone(tz).Date);

        var contracts = resourceWithContract.EmployeeId.GetContracts(
            dateStart,
            dateEnd,
            states: new[] { "open", "draft", "close" }
        ).Where(c => c.State == "open" || c.State == "close" || c.KanbanState == "done");

        foreach (var contract in contracts)
        {
            var tz = contract.EmployeeId.Tz;
            var resourceId = contract.EmployeeId.ResourceId.Id;
            var calendar = contract.ResourceCalendarId;

            if (!calendarsWithinPeriodPerResource.ContainsKey(resourceId))
                calendarsWithinPeriodPerResource[resourceId] = new Dictionary<ResourceCalendar, Intervals>();

            if (!calendarsWithinPeriodPerResource[resourceId].ContainsKey(calendar))
                calendarsWithinPeriodPerResource[resourceId][calendar] = new Intervals();

            var contractStart = contract.DateStart > start.ToZone(tz).Date
                ? contract.DateStart.AtStartOfDay().ToZone(tz)
                : start;

            var contractEnd = contract.DateEnd.HasValue && contract.DateEnd.Value < end.ToZone(tz).Date
                ? contract.DateEnd.Value.AtEndOfDay().ToZone(tz)
                : end;

            calendarsWithinPeriodPerResource[resourceId][calendar].Add(new Interval(
                contractStart,
                contractEnd,
                Env.ResourceCalendarAttendance
            ));
        }

        return calendarsWithinPeriodPerResource;
    }
}
