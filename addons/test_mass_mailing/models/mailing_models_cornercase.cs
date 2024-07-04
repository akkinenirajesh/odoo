C#
public partial class MailingTestPartnerUnstored {
    public void ComputePartnerId() {
        var partners = Env.Get<ResPartner>().Search(
            Env.NewDomain("EmailNormalized", "in", this.Filtered("EmailFrom").Mapped("EmailNormalized")));
        this.PartnerId = null;
        foreach (var record in this.Filtered("EmailFrom")) {
            record.PartnerId = partners.FirstOrDefault(
                p => p.EmailNormalized == record.EmailNormalized
            )?.Id;
        }
    }
}
