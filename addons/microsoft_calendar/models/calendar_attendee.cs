csharp
public partial class CalendarAttendee {
    public CalendarAttendee DoTentative() {
        // Synchronize event after state change
        CalendarAttendee res = Env.CallMethod<CalendarAttendee>("do_tentative", this);
        _MicrosoftSyncEvent("tentativelyAccept");
        return res;
    }

    public CalendarAttendee DoAccept() {
        // Synchronize event after state change
        CalendarAttendee res = Env.CallMethod<CalendarAttendee>("do_accept", this);
        _MicrosoftSyncEvent("accept");
        return res;
    }

    public CalendarAttendee DoDecline() {
        // Synchronize event after state change
        CalendarAttendee res = Env.CallMethod<CalendarAttendee>("do_decline", this);
        _MicrosoftSyncEvent("decline");
        return res;
    }

    private void _MicrosoftSyncEvent(string answer) {
        Dictionary<string, object> params = new Dictionary<string, object>() { { "comment", "" }, { "sendResponse", true } };
        // Microsoft prevent user to answer the meeting when they are the organizer
        var linkedEvents = Env.CallMethod<List<CalendarEvent>>("_get_synced_events", this.EventId);
        foreach (var event in linkedEvents) {
            if (event.CheckMicrosoftSyncStatus() && Env.User != event.UserId && Env.User.PartnerId.IsIn(event.PartnerIds)) {
                if (event.Recurrency) {
                    event._ForbidRecurrenceUpdate();
                }
                event._MicrosoftAttendeeAnswer(answer, params);
            }
        }
    }
}
