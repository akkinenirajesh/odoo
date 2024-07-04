csharp
public partial class Attendee
{
    public void DoTentative()
    {
        // Synchronize event after state change
        State = AttendeeState.Tentative;
        SyncEvent();
    }

    public void DoAccept()
    {
        // Synchronize event after state change
        State = AttendeeState.Accepted;
        SyncEvent();
    }

    public void DoDecline()
    {
        // Synchronize event after state change
        State = AttendeeState.Declined;
        SyncEvent();
    }

    private void SyncEvent()
    {
        if (Event?.GoogleId == null)
        {
            return;
        }

        var allEvents = new List<Calendar.Event> { Event };
        var otherEvents = allEvents.Where(e => e.User != null && e.User.Id != Env.User.Id).ToList();

        foreach (var user in otherEvents.Select(e => e.User).Distinct())
        {
            var service = new GoogleCalendarService(Env.GetService<GoogleService>().WithUser(user));
            var userEvents = otherEvents.Where(ev => ev.User.Id == user.Id).ToList();
            SyncOdoo2Google(userEvents, service);
        }

        var googleService = new GoogleCalendarService(Env.GetService<GoogleService>());
        var remainingEvents = allEvents.Except(otherEvents).ToList();
        SyncOdoo2Google(remainingEvents, googleService);
    }

    private void SyncOdoo2Google(List<Calendar.Event> events, GoogleCalendarService service)
    {
        // Implementation of syncing events with Google Calendar
        // This would need to be adapted based on your specific requirements and the GoogleCalendarService implementation
    }
}
