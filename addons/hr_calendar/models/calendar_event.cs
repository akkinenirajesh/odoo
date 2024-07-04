csharp
public partial class CalendarEvent
{
    public void ComputeUnavailablePartnerIds()
    {
        var eventIntervals = GetEventsInterval();

        foreach (var eventInterval in eventIntervals)
        {
            var @event = eventInterval.Key;
            var interval = eventInterval.Value;

            if (interval == null || !@event.PartnerIds.Any())
            {
                @event.UnavailablePartnerIds = new List<ResPartner>();
                continue;
            }

            var start = interval.Items[0].Start;
            var stop = interval.Items[0].End;
            var scheduleByPartner = @event.PartnerIds.GetSchedule(start, stop, merge: false);
            @event.UnavailablePartnerIds = CheckEmployeesAvailabilityForEvent(scheduleByPartner, interval);
        }
    }

    public List<DateTime> GetUnusualDays(DateTime dateFrom, DateTime? dateTo = null)
    {
        return Env.User.EmployeeId.GetUnusualDays(dateFrom, dateTo);
    }

    private Dictionary<CalendarEvent, Interval> GetEventsInterval()
    {
        var start = this.Start.Date;
        var stop = this.Stop.Date.AddDays(1).AddSeconds(-1);

        if (start == DateTime.MinValue || stop == DateTime.MinValue)
        {
            return new Dictionary<CalendarEvent, Interval>();
        }

        var companyCalendar = Env.Company.ResourceCalendarId;
        var globalInterval = companyCalendar.WorkIntervalsBatch(start, stop)[false];
        var intervalByEvent = new Dictionary<CalendarEvent, Interval>();

        var @event = this;
        var eventInterval = new Interval(new List<(DateTime, DateTime, ResourceCalendar)>
        {
            (TimeZoneInfo.ConvertTimeToUtc(@event.Start), TimeZoneInfo.ConvertTimeToUtc(@event.Stop), null)
        });

        if (@event.Allday)
        {
            intervalByEvent[@event] = eventInterval.Intersect(globalInterval);
        }
        else if (@event.Start != DateTime.MinValue && @event.Stop != DateTime.MinValue)
        {
            intervalByEvent[@event] = eventInterval;
        }

        return intervalByEvent;
    }

    private List<ResPartner> CheckEmployeesAvailabilityForEvent(Dictionary<ResPartner, Schedule> scheduleByPartner, Interval eventInterval)
    {
        var unavailablePartners = new List<ResPartner>();

        foreach (var kvp in scheduleByPartner)
        {
            var partner = kvp.Key;
            var schedule = kvp.Value;

            var commonInterval = schedule.Intersect(eventInterval);
            if (SumIntervals(commonInterval) != SumIntervals(eventInterval))
            {
                unavailablePartners.Add(partner);
            }
        }

        return unavailablePartners;
    }

    private TimeSpan SumIntervals(Interval interval)
    {
        // Implementation of sum_intervals logic
        throw new NotImplementedException();
    }
}
