C#
public partial class MassMailing {
    public bool UseLeads { get; set; }
    public int CrmLeadCount { get; set; }

    public void ComputeUseLeads() {
        this.UseLeads = Env.User.HasGroup("crm.group_use_lead");
    }

    public void ComputeCrmLeadCount() {
        var leadData = Env.Model("crm.lead").SearchRead(
            new[] { new Search("SourceId", "in", this.SourceId.Ids) },
            new[] { "SourceId" },
            new[] { "Count" });
        var mappedData = leadData.Select(x => new { (int)x["SourceId"], (int)x["Count"] })
            .ToDictionary(x => x.SourceId, x => x.Count);
        this.CrmLeadCount = mappedData.ContainsKey(this.SourceId.Id) ? mappedData[this.SourceId.Id] : 0;
    }

    public dynamic ActionRedirectToLeadsAndOpportunities() {
        var text = this.UseLeads ? "Leads" : "Opportunities";
        var helperHeader = $"No {text} yet!";
        var helperMessage = "Note that Odoo cannot track replies if they are sent towards email addresses to this database.";
        return new {
            Context = new {
                ActiveTest = false,
                Create = false,
                SearchDefaultGroupByCreateDateDay = true,
                CrmLeadViewHideMonth = true,
            },
            Domain = new[] { new Search("SourceId", "in", this.SourceId.Ids) },
            Help = $"<p class=\"o_view_nocontent_smiling_face\">{helperHeader}</p><p>{helperMessage}</p>",
            Name = "Leads Analysis",
            ResModel = "crm.lead",
            Type = "ir.actions.act_window",
            ViewMode = "tree,pivot,graph,form",
        };
    }

    public dynamic PrepareStatisticsEmailValues() {
        var values = base.PrepareStatisticsEmailValues();
        if (this.UserId == null) {
            return values;
        }
        if (!Env.Model("crm.lead").CheckAccessRights("read", raiseException: false)) {
            return values;
        }
        values["kpi_data"][1]["kpi_col1"] = new {
            Value = Env.FormatDecimalizedNumber(this.CrmLeadCount, decimal: 0),
            ColSubtitle = "LEADS",
        };
        values["kpi_data"][1]["kpi_name"] = "lead";
        return values;
    }
}
