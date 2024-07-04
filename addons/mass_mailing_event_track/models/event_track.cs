csharp
public partial class MassMailingEventTrack.EventTrack 
{
    public virtual List<MassMailing.MailMessage> MailingGetDefaultDomain(MassMailing.Mailing mailing)
    {
        return Env.Search<MassMailingEventTrack.EventTrack>(x => x.StageId.IsCancel == false);
    }
}
