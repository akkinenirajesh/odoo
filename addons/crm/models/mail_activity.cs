csharp
public partial class Activity
{
    public ActionResult ActionCreateCalendarEvent()
    {
        // Small override of the action that creates a calendar.
        // If the activity is linked to a crm.lead through the "opportunity_id" field, we include in
        // the action context the default values used when scheduling a meeting from the crm.lead form
        // view.
        // e.g: It will set the partner_id of the crm.lead as default attendee of the meeting.

        var action = base.ActionCreateCalendarEvent();
        var opportunity = this.CalendarEventId?.OpportunityId;
        
        if (opportunity != null)
        {
            var opportunityActionContext = opportunity.ActionScheduleMeeting(smartCalendar: false).Context ?? new Dictionary<string, object>();
            opportunityActionContext["initial_date"] = this.CalendarEventId.Start;

            foreach (var kvp in opportunityActionContext)
            {
                action.Context[kvp.Key] = kvp.Value;
            }
        }

        return action;
    }
}
