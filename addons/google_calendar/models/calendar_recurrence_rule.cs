csharp
public partial class RecurrenceRule
{
    public CalendarEvent[] ApplyRecurrence(Dictionary<string, object> specificValuesCreation = null, bool noSendEdit = false, Dictionary<string, object> genericValuesCreation = null)
    {
        var events = this.CalendarEventIds.Where(e => e.NeedSync);
        var detachedEvents = base.ApplyRecurrence(specificValuesCreation, noSendEdit, genericValuesCreation);

        var googleService = new GoogleCalendarService(Env.Get<GoogleService>());

        var vals = new List<Dictionary<string, object>>();
        foreach (var evt in events.Where(e => !string.IsNullOrEmpty(e.GoogleId)))
        {
            if (evt.Active && evt.GoogleId != GetEventGoogleId(evt))
            {
                vals.Add(new Dictionary<string, object>
                {
                    {"Name", evt.Name},
                    {"GoogleId", evt.GoogleId},
                    {"Start", evt.Start},
                    {"Stop", evt.Stop},
                    {"Active", false},
                    {"NeedSync", true}
                });
                evt.GoogleDelete(googleService, evt.GoogleId);
                evt.GoogleId = null;
            }
        }
        Env.Get<CalendarEvent>().Create(vals);

        foreach (var evt in this.CalendarEventIds)
        {
            evt.NeedSync = false;
        }

        return detachedEvents;
    }

    public string GetEventGoogleId(CalendarEvent evt)
    {
        if (!string.IsNullOrEmpty(this.GoogleId))
        {
            string timeId;
            if (evt.Allday)
            {
                timeId = evt.StartDate.ToString("yyyyMMdd");
            }
            else
            {
                timeId = evt.Start.ToString("yyyyMMddTHHmmssZ");
            }
            return $"{this.GoogleId}_{timeId}";
        }
        return null;
    }

    public void WriteEvents(Dictionary<string, object> values, DateTime? dtstart = null)
    {
        values.Remove("GoogleId");
        values["NeedSync"] = false;
        base.WriteEvents(values, dtstart);
    }

    public void Cancel()
    {
        foreach (var evt in this.CalendarEventIds)
        {
            evt.Cancel();
        }
        base.Cancel();
    }

    public string[] GetGoogleSyncedFields()
    {
        return new[] { "Rrule" };
    }

    public void WriteFromGoogle(GoogleEvent gevent, Dictionary<string, object> vals)
    {
        string currentRrule = this.Rrule;
        vals["EventTz"] = gevent.Start.TimeZone;
        base.WriteFromGoogle(gevent, vals);

        // ... (rest of the method implementation)
    }

    public Dictionary<string, object> GoogleValues()
    {
        var evt = GetFirstEvent();
        if (evt == null)
        {
            return new Dictionary<string, object>();
        }

        var values = evt.GoogleValues();
        values["id"] = this.GoogleId;
        if (!IsAllday())
        {
            values["start"]["timeZone"] = this.EventTz ?? "Etc/UTC";
            values["end"]["timeZone"] = this.EventTz ?? "Etc/UTC";
        }

        string rrule = Regex.Replace(this.Rrule, @"DTSTART:[0-9]{8}T[0-9]{1,8}\n", "");
        rrule = Regex.Replace(rrule, @"(UNTIL=\d{8}T\d{6})($|;)", "$1Z$2");
        values["recurrence"] = new[] { rrule.StartsWith("RRULE:") ? rrule : $"RRULE:{rrule}" };

        string propertyLocation = evt.UserId != null ? "shared" : "private";
        values["extendedProperties"] = new Dictionary<string, object>
        {
            [propertyLocation] = new Dictionary<string, object>
            {
                [$"{Env.DbName}_odoo_id"] = this.Id
            }
        };

        return values;
    }
}
