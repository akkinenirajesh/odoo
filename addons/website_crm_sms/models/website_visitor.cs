csharp
public partial class WebsiteVisitor {
    public bool CheckForSmsComposer() {
        var check = Env.Call("website.visitor", "_check_for_sms_composer", this);
        if (!check && this.LeadIds.Count > 0) {
            var sortedLeads = this.LeadIds.Where(l => l.Mobile == this.Mobile || l.Phone == this.Mobile).OrderByDescending(l => l.ConfidenceLevel);
            if (sortedLeads.Count() > 0) {
                return true;
            }
        }
        return check;
    }

    public object PrepareSmsComposerContext() {
        if (!this.PartnerId && this.LeadIds.Count > 0) {
            var leadsWithNumber = this.LeadIds.Where(l => l.Mobile == this.Mobile || l.Phone == this.Mobile).OrderByDescending(l => l.ConfidenceLevel);
            if (leadsWithNumber.Count() > 0) {
                var lead = leadsWithNumber.First();
                return new {
                    default_res_model = "crm.lead",
                    default_res_id = lead.Id,
                    number_field_name = lead.Mobile == this.Mobile ? "mobile" : "phone"
                };
            }
        }
        return Env.Call("website.visitor", "_prepare_sms_composer_context", this);
    }
}
