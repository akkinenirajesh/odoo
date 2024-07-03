csharp
public partial class Partner
{
    public int ComputeMeetingCount()
    {
        var result = ComputeMeeting();
        return result.ContainsKey(this.Id) ? result[this.Id].Count : 0;
    }

    public Dictionary<int, List<int>> ComputeMeeting()
    {
        if (this.Id == 0) return new Dictionary<int, List<int>>();

        var allPartners = Env.Partner.WithContext(new { active_test = false })
            .SearchFetch(new[] { ("id", "child_of", this.Id) }, new[] { "ParentId" });

        var query = Env.CalendarEvent.Search(new object[] { });  // ir.rules will be applied
        var meetingData = Env.ExecuteQuery(SQL(
            @"SELECT res_partner_id, calendar_event_id, count(1)
              FROM calendar_event_res_partner_rel
              WHERE res_partner_id IN @p0 AND calendar_event_id IN @p1
              GROUP BY res_partner_id, calendar_event_id",
            allPartners.Select(p => p.Id).ToArray(),
            query.Subselect()
        ));

        var meetings = new Dictionary<int, HashSet<int>>();
        foreach (var (partnerId, meetingId, _) in meetingData)
        {
            if (!meetings.ContainsKey(partnerId))
                meetings[partnerId] = new HashSet<int>();
            meetings[partnerId].Add(meetingId);
        }

        foreach (var partner in Env.Partner.Browse(meetings.Keys))
        {
            var currentPartner = partner;
            while (currentPartner.ParentId != null)
            {
                currentPartner = currentPartner.ParentId;
                if (meetings.ContainsKey(currentPartner.Id))
                {
                    meetings[currentPartner.Id].UnionWith(meetings[partner.Id]);
                }
            }
        }

        return meetings.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList()
        );
    }

    public List<Dictionary<string, object>> GetAttendeeDetail(List<int> meetingIds)
    {
        var attendeesDetails = new List<Dictionary<string, object>>();
        var meetings = Env.CalendarEvent.Browse(meetingIds);

        foreach (var attendee in meetings.SelectMany(m => m.AttendeeIds))
        {
            if (!this.Equals(attendee.PartnerId)) continue;

            var attendeeIsOrganizer = Env.User.Equals(attendee.EventId.UserId) && attendee.PartnerId.Equals(Env.User.PartnerId);
            attendeesDetails.Add(new Dictionary<string, object>
            {
                { "Id", attendee.PartnerId.Id },
                { "Name", attendee.PartnerId.DisplayName },
                { "Status", attendee.State },
                { "EventId", attendee.EventId.Id },
                { "AttendeeId", attendee.Id },
                { "IsAlone", attendee.EventId.IsOrganizerAlone && attendeeIsOrganizer },
                { "IsOrganizer", attendee.PartnerId.Equals(attendee.EventId.UserId.PartnerId) ? 1 : 0 }
            });
        }

        return attendeesDetails;
    }

    public void SetCalendarLastNotifAck()
    {
        var partner = Env.User.PartnerId;
        partner.CalendarLastNotifAck = DateTime.Now;
    }

    public Dictionary<string, object> ScheduleMeeting()
    {
        var partnerIds = new List<int> { this.Id, Env.User.PartnerId.Id };
        var action = Env.IrActionsActions.ForXmlId("Calendar.ActionCalendarEvent");
        action["Context"] = new Dictionary<string, object>
        {
            { "default_partner_ids", partnerIds }
        };
        action["Domain"] = new List<object>
        {
            "|",
            new object[] { "Id", "in", ComputeMeeting()[this.Id] },
            new object[] { "PartnerIds", "in", new List<int> { this.Id } }
        };
        return action;
    }
}
