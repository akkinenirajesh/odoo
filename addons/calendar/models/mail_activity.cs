csharp
public partial class Activity
{
    public Dictionary<string, object> ActionCreateCalendarEvent()
    {
        var action = Env.Actions.ForXmlId("calendar.action_calendar_event");
        action["context"] = new Dictionary<string, object>
        {
            ["default_activity_type_id"] = this.ActivityTypeId.Id,
            ["default_res_id"] = Env.Context.GetValueOrDefault("default_res_id"),
            ["default_res_model"] = Env.Context.GetValueOrDefault("default_res_model"),
            ["default_name"] = this.Summary ?? this.ResName,
            ["default_description"] = string.IsNullOrWhiteSpace(this.Note) ? "" : this.Note,
            ["default_activity_ids"] = new List<int> { this.Id },
            ["default_partner_ids"] = new List<int> { this.UserId.PartnerId.Id },
            ["default_user_id"] = this.UserId.Id,
            ["initial_date"] = this.DateDeadline,
            ["default_calendar_event_id"] = this.CalendarEventId?.Id
        };
        return action;
    }

    public (List<Message>, List<Activity>) ActionDone(string feedback = null, List<int> attachmentIds = null)
    {
        var events = this.CalendarEventId;
        var (messages, activities) = base.ActionDone(feedback, attachmentIds);

        if (!string.IsNullOrEmpty(feedback))
        {
            foreach (var @event in events)
            {
                var description = @event.Description;
                description = string.IsNullOrWhiteSpace(description)
                    ? $"Feedback: {Env.Tools.Plaintext2Html(feedback)}"
                    : $"{description}<br />Feedback: {Env.Tools.Plaintext2Html(feedback)}";
                @event.Write(new { Description = description });
            }
        }

        return (messages, activities);
    }

    public bool UnlinkWMeeting()
    {
        var events = this.CalendarEventId;
        var res = this.Unlink();
        events.Unlink();
        return res;
    }
}
