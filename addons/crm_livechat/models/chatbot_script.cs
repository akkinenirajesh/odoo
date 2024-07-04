csharp
public partial class ChatbotScript
{
    public void _ComputeLeadCount()
    {
        var leadsData = Env.Get<CrmLead>().WithContext(new { active_test = false }).Sudo()
            .ReadGroup(
                new[] { ("SourceId", "in", new[] { this.SourceId.Id }) },
                new[] { "SourceId" },
                new[] { "__count" }
            );

        var mappedLeads = leadsData.ToDictionary(
            data => data.SourceId.Id,
            data => data.__count
        );

        this.LeadCount = mappedLeads.TryGetValue(this.SourceId.Id, out var count) ? count : 0;
    }

    public ActionResult ActionViewLeads()
    {
        var action = Env.Get<IrActionsActWindow>().ForXmlId("crm.crm_lead_all_leads");
        action.Domain = new[] { ("SourceId", "=", this.SourceId.Id) };
        action.Context = new { create = false };
        return action;
    }
}
