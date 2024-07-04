csharp
public partial class Event
{
    private const string MEET_ROUTE = "meet.google.com";

    public override string ToString()
    {
        return Name ?? "(No title)";
    }

    public void ComputeGoogleId()
    {
        if (string.IsNullOrEmpty(GoogleId) && RecurrenceId != null)
        {
            GoogleId = RecurrenceId.GetEventGoogleId(this);
        }
        else if (string.IsNullOrEmpty(GoogleId))
        {
            GoogleId = null;
        }
    }

    public void ComputeVideocallSource()
    {
        if (!string.IsNullOrEmpty(VideocallLocation) && VideocallLocation.Contains(MEET_ROUTE))
        {
            VideocallSource = VideocallSource.GoogleMeet;
        }
        else
        {
            // Call base implementation for other cases
            base.ComputeVideocallSource();
        }
    }

    public HashSet<string> GetGoogleSyncedFields()
    {
        return new HashSet<string> { "Name", "Description", "Allday", "Start", "DateEnd", "Stop",
                                     "Attendees", "Alarms", "Location", "Privacy", "Active", "ShowAs" };
    }

    public void RestartGoogleSync()
    {
        var events = Env.Query<Event>().Where(GetSyncDomain()).ToList();
        foreach (var evt in events)
        {
            evt.NeedSync = true;
        }
    }

    public bool CheckValuesToSync(Dictionary<string, object> values)
    {
        var syncedFields = GetGoogleSyncedFields();
        return values.Keys.Any(key => syncedFields.Contains(key));
    }

    public Dictionary<string, object> GetUpdateFutureEventsValues()
    {
        var baseValues = base.GetUpdateFutureEventsValues();
        baseValues["NeedSync"] = false;
        return baseValues;
    }

    public Dictionary<string, object> GetRemoveSyncIdValues()
    {
        var baseValues = base.GetRemoveSyncIdValues();
        baseValues["GoogleId"] = null;
        return baseValues;
    }

    public Dictionary<string, object> GetArchiveValues()
    {
        var baseValues = base.GetArchiveValues();
        baseValues["NeedSync"] = false;
        return baseValues;
    }

    public void CheckModifyEventPermission(Dictionary<string, object> values)
    {
        bool googleSyncRestart = values.ContainsKey("NeedSync") && values.Count == 1;
        if (!googleSyncRestart && GuestsReadonly && Env.User.Id != User.Id)
        {
            throw new ValidationException("The following event can only be updated by the organizer according to the event permissions set on Google Calendar.");
        }
    }

    public bool SkipSendMailStatusUpdate()
    {
        var userId = GetEventUser();
        if (userId.IsGoogleCalendarSynced() && userId.ResUsersSettings.IsGoogleCalendarValid())
        {
            return true;
        }
        return base.SkipSendMailStatusUpdate();
    }

    public List<object> GetSyncDomain()
    {
        int dayRange = int.Parse(Env.GetParameter("google_calendar.sync.range_days", "365"));
        var lowerBound = DateTime.Now.AddDays(-dayRange);
        var upperBound = DateTime.Now.AddDays(dayRange);

        return new List<object>
        {
            new List<object> { "Partners.Users", "in", Env.User.Id },
            new List<object> { "Stop", ">", lowerBound },
            new List<object> { "Start", "<", upperBound },
            "!",
            new List<object>
            {
                new List<object> { "Recurrency", "=", true },
                new List<object> { "RecurrenceId", "!=", null },
                new List<object> { "FollowRecurrence", "=", true }
            }
        };
    }

    // Other methods would be implemented similarly, adapting Odoo's Python logic to C#
}
