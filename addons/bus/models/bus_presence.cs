csharp
public partial class BusPresence
{
    private const int UPDATE_PRESENCE_DELAY = 60;
    private const int DISCONNECTION_TIMER = UPDATE_PRESENCE_DELAY + 5;
    private const int AWAY_TIMER = 1800; // 30 minutes

    public void UpdatePresence(int inactivityPeriod, string identityField, object identityValue)
    {
        try
        {
            // Hide transaction serialization errors, which can be ignored, the presence update is not essential
            // The errors are supposed from presence.Write(...) call only
            using (Env.MuteLogger("odoo.sql_db"))
            {
                UpdatePresenceInternal(inactivityPeriod, identityField, identityValue);
                // commit on success
                Env.Commit();
            }
        }
        catch (OperationalException e)
        {
            if (PG_CONCURRENCY_ERRORS_TO_RETRY.Contains(e.PgCode))
            {
                // ignore concurrency error
                Env.Rollback();
                return;
            }
            throw;
        }
    }

    private void UpdatePresenceInternal(int inactivityPeriod, string identityField, object identityValue)
    {
        var presence = Env.Query<BusPresence>().FirstOrDefault(p => p.GetType().GetProperty(identityField).GetValue(p) == identityValue);
        
        // compute last_presence timestamp
        var lastPresence = DateTime.Now.AddMilliseconds(-inactivityPeriod);
        var values = new Dictionary<string, object>
        {
            { "LastPoll", DateTime.Now }
        };

        // update the presence or create a new one
        if (presence == null)
        {
            values[identityField] = identityValue;
            values["LastPresence"] = lastPresence;
            Env.Create<BusPresence>(values);
        }
        else
        {
            if (presence.LastPresence < lastPresence)
            {
                values["LastPresence"] = lastPresence;
            }
            presence.Write(values);
        }
    }

    public void GcBusPresence()
    {
        var inactivePresences = Env.Query<BusPresence>().Where(p => !p.UserId.Active);
        foreach (var presence in inactivePresences)
        {
            presence.Delete();
        }
    }
}
