C#
public partial class WebsiteVisitor {
    public int LeadCount { get; set; }

    public void ComputeLeadCount() {
        this.LeadCount = this.LeadIds.Count;
    }

    public void ComputeEmailPhone() {
        if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Mobile)) {
            var leads = this.LeadIds.OrderByDescending(lead => lead.CreateDate).ToList();
            foreach (var lead in leads) {
                if (string.IsNullOrEmpty(this.Email)) {
                    this.Email = lead.EmailNormalized;
                }
                if (string.IsNullOrEmpty(this.Mobile)) {
                    this.Mobile = lead.Mobile ?? lead.Phone;
                }
            }
        }
    }

    public bool CheckForMessageComposer() {
        if (this.LeadIds.Any()) {
            var sortedLeads = this.LeadIds.OrderByDescending(lead => lead.ConfidenceLevel).ToList();
            var partners = sortedLeads.Select(lead => lead.PartnerId).ToList();
            if (partners.Any()) {
                return true;
            } else {
                var mainLead = this.LeadIds.FirstOrDefault();
                mainLead.HandlePartnerAssignment(true);
                this.PartnerId = mainLead.PartnerId.Id;
                return true;
            }
        }
        return false;
    }

    public Domain InactiveVisitorsDomain() {
        return new Domain(new List<DomainPart>()
        {
            new DomainPart() { FieldName = "LeadIds", Operator = "=", Value = false }
        });
    }

    public void MergeVisitor(WebsiteVisitor target) {
        if (this.LeadIds.Any()) {
            foreach (var lead in this.LeadIds) {
                target.LeadIds.Add(lead);
            }
        }
    }

    public Dictionary<string, object> PrepareMessageComposerContext() {
        if (this.PartnerId == null && this.LeadIds.Any()) {
            var sortedLeads = this.LeadIds.OrderByDescending(lead => lead.ConfidenceLevel).ToList();
            var leadPartners = sortedLeads.Select(lead => lead.PartnerId).ToList();
            var partner = leadPartners.FirstOrDefault();
            if (partner != null) {
                return new Dictionary<string, object>()
                {
                    { "DefaultModel", "crm.lead" },
                    { "DefaultResID", sortedLeads.FirstOrDefault().Id },
                    { "DefaultPartnerIds", partner.Id }
                };
            }
        }
        return new Dictionary<string, object>();
    }
}
