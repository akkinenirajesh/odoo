csharp
public partial class Event
{
    public object ActionMassMailingTrackSpeakers()
    {
        var massMailingAction = new
        {
            Name = "Mass Mail Attendees",
            Type = "ir.actions.act_window",
            ResModel = "mailing.mailing",
            ViewMode = "form",
            Target = "current",
            Context = new
            {
                DefaultMailingModelId = Env.Ref("website_event_track.model_event_track").Id,
                DefaultMailingDomain = $"[('event_id', 'in', this.Ids), ('stage_id.is_cancel', '!=', true)]",
                DefaultSubject = $"Event: {this.Name}"
            }
        };
        return massMailingAction;
    }
}
