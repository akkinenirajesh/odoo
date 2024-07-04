C#
public partial class Event {
    public virtual void ActionMassMailingAttendees() {
        var action = Env.Call("Event", "action_mass_mailing_attendees", this);
        action.ViewId = Env.Ref("mass_mailing_sms.mailing_mailing_view_form_mixed").Id;
        return action;
    }

    public virtual void ActionInviteContacts() {
        var action = Env.Call("Event", "action_invite_contacts", this);
        action.ViewId = Env.Ref("mass_mailing_sms.mailing_mailing_view_form_mixed").Id;
        return action;
    }
}
