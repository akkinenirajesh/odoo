csharp
public partial class Event {
    public Dictionary<string, object> ActionMassMailingAttendees() {
        return new Dictionary<string, object>() {
            { "name", "Mass Mail Attendees" },
            { "type", "ir.actions.act_window" },
            { "res_model", "mailing.mailing" },
            { "view_mode", "form" },
            { "target", "current" },
            { "context", new Dictionary<string, object>() {
                { "default_mailing_model_id", Env.Ref("event.model_event_registration").Id },
                { "default_mailing_domain", $"[{nameof(AttendeeIds)}: [in, this.Id], {nameof(State)}: [!=, 'cancel']]" },
                { "default_subject", $"Event: {this.Name}" },
            } },
        };
    }

    public Dictionary<string, object> ActionInviteContacts() {
        return new Dictionary<string, object>() {
            { "name", "Mass Mail Invitation" },
            { "type", "ir.actions.act_window" },
            { "res_model", "mailing.mailing" },
            { "view_mode", "form" },
            { "target", "current" },
            { "context", new Dictionary<string, object>() {
                { "default_mailing_model_id", Env.Ref("base.model_res_partner").Id },
                { "default_subject", $"Event: {this.Name}" },
            } },
        };
    }
}
