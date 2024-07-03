csharp
public partial class ResUsers
{
    public List<int> GetSelectedCalendarsPartnerIds(bool includeUser = true)
    {
        var partnerIds = Env.Set<Calendar.CalendarFilters>()
            .Search(f => f.UserId == this.Id && f.PartnerChecked)
            .Select(f => f.PartnerId)
            .ToList();

        if (includeUser)
        {
            partnerIds.Add(Env.User.PartnerId);
        }

        return partnerIds;
    }

    public List<object> SystrayGetCalendarEventDomain()
    {
        var nowUtc = DateTime.UtcNow;
        var startDtUtc = nowUtc;
        var stopDtUtc = nowUtc.Date.Add(TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)));

        var startDt = startDtUtc;
        var stopDt = stopDtUtc;

        if (!string.IsNullOrEmpty(Env.User.TimeZone))
        {
            var userTz = TimeZoneInfo.FindSystemTimeZoneById(Env.User.TimeZone);
            startDt = TimeZoneInfo.ConvertTimeFromUtc(startDtUtc, userTz);
            stopDt = startDt.Date.Add(TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)));
            stopDtUtc = TimeZoneInfo.ConvertTimeToUtc(stopDt, userTz);
        }

        var startDate = startDt.Date;

        var currentUserNonDeclinedAttendeeIds = Env.Set<Calendar.Attendee>()
            .Search(a => a.PartnerId == Env.User.PartnerId && a.State != "declined")
            .Select(a => a.Id)
            .ToList();

        return new List<object>
        {
            "&", "|",
            "&",
                "|",
                    new List<object> { "Start", ">=", startDtUtc },
                    new List<object> { "Stop", ">=", startDtUtc },
                new List<object> { "Start", "<=", stopDtUtc },
            "&",
                new List<object> { "Allday", "=", true },
                new List<object> { "StartDate", "=", startDate },
            new List<object> { "AttendeeIds", "in", currentUserNonDeclinedAttendeeIds }
        };
    }

    public Dictionary<string, object> GetActivityGroups()
    {
        var result = base.GetActivityGroups();
        var eventModel = Env.Set<Calendar.Event>();
        var meetingsLines = eventModel.SearchRead(
            SystrayGetCalendarEventDomain(),
            new[] { "Id", "Start", "Name", "Allday" },
            orderBy: "Start"
        );

        if (meetingsLines.Any())
        {
            var meetingLabel = "Today's Meetings";
            var meetingsSystray = new Dictionary<string, object>
            {
                { "Id", Env.Set<Core.IrModel>().GetId("Calendar.Event") },
                { "Type", "meeting" },
                { "Name", meetingLabel },
                { "Model", "Calendar.Event" },
                { "Icon", Env.GetModuleIcon(eventModel.OriginalModule) },
                { "Meetings", meetingsLines },
                { "ViewType", eventModel.SystrayView }
            };
            result.Insert(0, meetingsSystray);
        }

        return result;
    }

    public Dictionary<string, object> CheckCalendarCredentials()
    {
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> CheckSynchronizationStatus()
    {
        return new Dictionary<string, object>();
    }
}
