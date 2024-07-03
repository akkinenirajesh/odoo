csharp
public partial class Attendee
{
    public override string ToString()
    {
        return CommonName ?? Email;
    }

    public void ComputeCommonName()
    {
        CommonName = Partner?.Name ?? Email;
    }

    public void ComputeMailTz()
    {
        MailTz = Partner?.Tz;
    }

    public void DoTentative()
    {
        State = AttendeeState.Tentative;
    }

    public void DoAccept()
    {
        Event?.MessagePost(
            authorId: Partner?.Id,
            body: $"{CommonName} has accepted the invitation",
            subtypeXmlid: "Calendar.SubtypeInvitation"
        );
        State = AttendeeState.Accepted;
    }

    public void DoDecline()
    {
        Event?.MessagePost(
            authorId: Partner?.Id,
            body: $"{CommonName} has declined the invitation",
            subtypeXmlid: "Calendar.SubtypeInvitation"
        );
        State = AttendeeState.Declined;
    }

    public bool ShouldNotifyAttendee()
    {
        return Partner?.Id != Env.User.PartnerId;
    }

    // Other methods like SendInvitationEmails, SendMailToAttendees, etc. would be implemented here
    // These methods might require more complex logic and interaction with other services
}
