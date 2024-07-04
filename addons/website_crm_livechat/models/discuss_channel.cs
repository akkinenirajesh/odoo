csharp
public partial class WebsiteCrmLivechat_DiscussChannel {
    public WebsiteCrmLivechat_LivechatVisitor LivechatVisitorId { get; set; }
    public Crm_Lead[] LeadIds { get; set; }

    public Crm_Lead ConvertVisitorToLead(Crm_Partner partner, string key) {
        Crm_Lead lead = Env.Call("DiscussChannel", "_convert_visitor_to_lead", partner, key);
        WebsiteCrmLivechat_LivechatVisitor visitorSudo = Env.Call("DiscussChannel", "LivechatVisitorId").FirstOrDefault();
        if (visitorSudo != null) {
            visitorSudo.LeadIds = visitorSudo.LeadIds.Concat(new[] { lead }).ToArray();
            lead.CountryId = lead.CountryId ?? visitorSudo.CountryId;
        }
        return lead;
    }
}
