csharp
public partial class Campaign
{
    public bool ComputeUseLeads()
    {
        return Env.User.HasGroup("crm.group_use_lead");
    }

    public int ComputeCrmLeadCount()
    {
        var leadData = Env.Get<CrmLead>().WithContext(new { active_test = false })
            .ReadGroup(
                domain: new[] { ("CampaignId", "in", new[] { Id }) },
                fields: new[] { "CampaignId" },
                groupby: new[] { "__count" }
            );

        return leadData.FirstOrDefault()?.Count ?? 0;
    }

    public ActionResult ActionRedirectToLeadsOpportunities()
    {
        string view = UseLeads ? "crm.crm_lead_all_leads" : "crm.crm_lead_opportunities";
        var action = Env.Get<IrActionsActWindow>().ForXmlId(view);
        action.ViewMode = "tree,kanban,graph,pivot,form,calendar";
        action.Domain = new[] { ("CampaignId", "in", new[] { Id }) };
        action.Context = new Dictionary<string, object>
        {
            { "active_test", false },
            { "create", false }
        };
        return action;
    }
}
