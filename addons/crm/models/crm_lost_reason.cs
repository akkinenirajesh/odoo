csharp
public partial class LostReason
{
    public void ComputeLeadsCount()
    {
        var leadData = Env.Get<CrmLead>().WithContext(new { active_test = false })
            .ReadGroup(
                new[] { new[] { "LostReason", "in", new[] { this.Id } } },
                new[] { "LostReason" },
                new[] { "__count" }
            );

        var mappedData = leadData.ToDictionary(
            item => item.LostReason.Id,
            item => (int)item.__count
        );

        this.LeadsCount = mappedData.GetValueOrDefault(this.Id, 0);
    }

    public Dictionary<string, object> ActionLostLeads()
    {
        return new Dictionary<string, object>
        {
            { "name", "Leads" },
            { "view_mode", "tree,form" },
            { "domain", new[] { new[] { "LostReason", "in", new[] { this.Id } } } },
            { "res_model", "Crm.Lead" },
            { "type", "ir.actions.act_window" },
            { "context", new { create = false, active_test = false } }
        };
    }
}
