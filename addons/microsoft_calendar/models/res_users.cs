csharp
public partial class MicrosoftCalendar.User
{
    public bool IsMicrosoftCalendarAuthenticated()
    {
        return Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarRToken != null;
    }

    public string GetMicrosoftCalendarToken()
    {
        if (Env.Ref<MicrosoftCalendar.User>("this") == null)
        {
            return null;
        }

        if (Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarRToken != null && !IsMicrosoftCalendarValid())
        {
            RefreshMicrosoftCalendarToken();
        }

        return Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarToken;
    }

    public bool IsMicrosoftCalendarValid()
    {
        return Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarTokenValidity != null && Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarTokenValidity >= (DateTime.Now + TimeSpan.FromMinutes(1));
    }

    public void RefreshMicrosoftCalendarToken()
    {
        string clientId = Env.Ref<Ir.ConfigParameter>("microsoft_calendar_client_id");
        string clientSecret = Env.Ref<Ir.ConfigParameter>("microsoft_calendar_client_secret");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("The account for the Outlook Calendar service is not configured.");
        }

        var headers = new Dictionary<string, string>() { { "content-type", "application/x-www-form-urlencoded" } };
        var data = new Dictionary<string, string>()
        {
            { "refresh_token", Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarRToken },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "refresh_token" }
        };

        try
        {
            var response = Env.Ref<MicrosoftService.Service>().DoRequest(DEFAULT_MICROSOFT_TOKEN_ENDPOINT, data, headers, "POST", "").Result;
            var ttl = response.Get("expires_in");
            Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarToken = response.Get("access_token");
            Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarTokenValidity = DateTime.Now + TimeSpan.FromSeconds(Convert.ToInt32(ttl));
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("invalid grant") || ex.Message.Contains("invalid client"))
            {
                Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarRToken = null;
                Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarToken = null;
                Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarTokenValidity = null;
                Env.Ref<MicrosoftCalendar.ResUsersSettings>("this").MicrosoftCalendarSyncToken = null;
            }
            throw new Exception($"An error occurred while generating the token. Your authorization code may be invalid or has already expired [{ex.Message}]. You should check your Client ID and secret on the Microsoft Azure portal or try to stop and restart your calendar synchronisation.");
        }
    }

    public string GetMicrosoftSyncStatus()
    {
        string status = "sync_active";
        if (Env.Ref<Ir.ConfigParameter>("microsoft_calendar_sync_paused") == "true")
        {
            status = "sync_paused";
        }
        else if (Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarToken != null && !Env.Ref<MicrosoftCalendar.User>("this").MicrosoftSynchronizationStopped)
        {
            status = "sync_active";
        }
        else if (Env.Ref<MicrosoftCalendar.User>("this").MicrosoftSynchronizationStopped)
        {
            status = "sync_stopped";
        }
        return status;
    }

    public bool SyncMicrosoftCalendar()
    {
        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftLastSyncDate = DateTime.Now;
        if (GetMicrosoftSyncStatus() != "sync_active")
        {
            return false;
        }

        var calendarService = Env.Ref<Calendar.Event>().GetMicrosoftService();
        bool fullSync = Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarSyncToken == null;
        string nextSyncToken = null;
        List<object> events = new List<object>();
        try
        {
            using (var token = new MicrosoftCalendarToken(Env.Ref<MicrosoftCalendar.User>("this")))
            {
                events = calendarService.GetEvents(Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarSyncToken, token.Token).Result;
                nextSyncToken = calendarService.NextSyncToken;
            }
        }
        catch (Exception)
        {
            events = calendarService.GetEvents(token: null).Result;
            fullSync = true;
        }
        Env.Ref<MicrosoftCalendar.ResUsersSettings>("this").MicrosoftCalendarSyncToken = nextSyncToken;

        // Microsoft -> Odoo
        var syncedEvents = new List<object>();
        var syncedRecurrences = new List<object>();
        if (events.Count > 0)
        {
            (syncedEvents, syncedRecurrences) = Env.Ref<Calendar.Event>().SyncMicrosoft2Odoo(events);
        }

        // Odoo -> Microsoft
        var recurrences = Env.Ref<Calendar.Recurrence>().GetMicrosoftRecordsToSync(fullSync);
        recurrences.RemoveAll(r => syncedRecurrences.Contains(r));
        recurrences.ForEach(r => r.SyncOdoo2Microsoft());
        syncedEvents.AddRange(recurrences.Select(r => r.CalendarEventId));

        var eventsToSync = Env.Ref<Calendar.Event>().GetMicrosoftRecordsToSync(fullSync);
        eventsToSync.RemoveAll(e => syncedEvents.Contains(e));
        eventsToSync.ForEach(e => e.SyncOdoo2Microsoft());

        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftLastSyncDate = DateTime.Now;

        return (eventsToSync.Count > 0 || syncedEvents.Count > 0) || (recurrences.Count > 0 || syncedRecurrences.Count > 0);
    }

    public static void SyncAllMicrosoftCalendar()
    {
        var users = Env.Ref<MicrosoftCalendar.User>().Search(x => x.MicrosoftCalendarRToken != null && !x.MicrosoftSynchronizationStopped);
        foreach (var user in users)
        {
            Env.Log.Info($"Calendar Synchro - Starting synchronization for {user}");
            try
            {
                user.SyncMicrosoftCalendar();
                Env.Commit();
            }
            catch (Exception ex)
            {
                Env.Log.Error($"[{user}] Calendar Synchro - Exception : {ex.Message}!");
                Env.Rollback();
            }
        }
    }

    public void StopMicrosoftSynchronization()
    {
        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftSynchronizationStopped = true;
        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftLastSyncDate = null;
    }

    public void RestartMicrosoftSynchronization()
    {
        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftLastSyncDate = DateTime.Now;
        Env.Ref<MicrosoftCalendar.User>("this").MicrosoftSynchronizationStopped = false;
        Env.Ref<Calendar.Recurrence>().RestartMicrosoftSync();
        Env.Ref<Calendar.Event>().RestartMicrosoftSync();
    }

    public void UnpauseMicrosoftSynchronization()
    {
        Env.Ref<Ir.ConfigParameter>("microsoft_calendar_sync_paused") = "false";
    }

    public void PauseMicrosoftSynchronization()
    {
        Env.Ref<Ir.ConfigParameter>("microsoft_calendar_sync_paused") = "true";
    }

    public Dictionary<string, bool> CheckCalendarCredentials()
    {
        var res = new Dictionary<string, bool>() { { "microsoft_calendar", false } };

        var clientId = Env.Ref<Ir.ConfigParameter>("microsoft_calendar_client_id");
        var clientSecret = Env.Ref<Ir.ConfigParameter>("microsoft_calendar_client_secret");
        res["microsoft_calendar"] = !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret);
        return res;
    }

    public Dictionary<string, string> CheckSynchronizationStatus()
    {
        var res = new Dictionary<string, string>() { { "microsoft_calendar", "missing_credentials" } };

        var credentialsStatus = CheckCalendarCredentials();
        if (credentialsStatus["microsoft_calendar"])
        {
            res["microsoft_calendar"] = GetMicrosoftSyncStatus();
            if (res["microsoft_calendar"] == "sync_active" && Env.Ref<MicrosoftCalendar.User>("this").MicrosoftCalendarToken == null)
            {
                res["microsoft_calendar"] = "sync_stopped";
            }
        }
        return res;
    }
}
