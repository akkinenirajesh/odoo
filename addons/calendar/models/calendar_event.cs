csharp
public partial class CalendarEvent
{
    public override string ToString()
    {
        return Name;
    }

    public void SetDiscussVideocallLocation()
    {
        // Implementation of set_discuss_videocall_location method
    }

    public void ClearVideocallLocation()
    {
        // Implementation of clear_videocall_location method
    }

    public Core.Action ActionOpenCalendarEvent()
    {
        // Implementation of action_open_calendar_event method
        return null;
    }

    public bool ActionSendmail()
    {
        // Implementation of action_sendmail method
        return true;
    }

    public Core.Action ActionOpenComposer()
    {
        // Implementation of action_open_composer method
        return null;
    }

    public Core.Action ActionJoinVideoCall()
    {
        // Implementation of action_join_video_call method
        return null;
    }

    public void ActionJoinMeeting(int partnerId)
    {
        // Implementation of action_join_meeting method
    }

    public void ActionMassDeletion(string recurrenceUpdateSetting)
    {
        // Implementation of action_mass_deletion method
    }

    public void ActionMassArchive(string recurrenceUpdateSetting)
    {
        // Implementation of action_mass_archive method
    }

    // Add other methods as needed
}
