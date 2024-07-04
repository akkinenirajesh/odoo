csharp
public partial class MassMailing.MailBlackList {
    public MassMailing.MailingSubscriptionOptOut OptOutReasonId { get; set; }

    public MailBlackList TrackSubtype(Dictionary<string, object> initValues) {
        if (initValues.ContainsKey("OptOutReasonId") && this.OptOutReasonId != null) {
            return Env.Ref("mail.mt_comment");
        }
        return this.SuperTrackSubtype(initValues);
    }

    public MailBlackList SuperTrackSubtype(Dictionary<string, object> initValues) {
        // Call super method
        return this;
    }
}
