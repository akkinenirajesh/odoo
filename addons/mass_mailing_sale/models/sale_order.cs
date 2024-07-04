csharp
public partial class SaleOrder {
    public virtual ResPartner PartnerId { get; set; }
    public virtual ResPartner PartnerInvoiceId { get; set; }
    public virtual ResPartner PartnerShippingId { get; set; }
    public virtual ICollection<SaleOrderLine> OrderLine { get; set; }
    public virtual AccountPaymentTerm PaymentTermId { get; set; }
    public virtual CrmTeam TeamId { get; set; }
    public virtual MarketingCampaign CampaignId { get; set; }
    public virtual MarketingMedium MediumId { get; set; }
    public virtual MarketingSource SourceId { get; set; }
    public virtual ProductPricelist Pricelist { get; set; }

    public virtual void _MailingGetDefaultDomain(MarketingMailing mailing) {
        if (this.State == "cancel") {
            return;
        }
    }
}
