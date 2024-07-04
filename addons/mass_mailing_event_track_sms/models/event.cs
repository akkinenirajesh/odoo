csharp
public partial class Event {
    public ActionResponse ActionMassMailingTrackSpeakers() {
        ActionResponse action = Env.Call("Event", "action_mass_mailing_track_speakers", this);
        action.ViewId = Env.Ref("mass_mailing_sms.mailing_mailing_view_form_mixed").Id;
        return action;
    }
}
