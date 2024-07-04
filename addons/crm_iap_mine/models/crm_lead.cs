csharp
public partial class Lead
{
    public Dictionary<string, object> ActionGenerateLeads()
    {
        return new Dictionary<string, object>
        {
            ["name"] = "Need help reaching your target?",
            ["type"] = "ir.actions.act_window",
            ["res_model"] = "Crm.IapLeadMiningRequest",
            ["target"] = "new",
            ["views"] = new object[] { new object[] { false, "form" } },
            ["context"] = new Dictionary<string, object> { ["is_modal"] = true }
        };
    }

    public List<string> MergeGetFields()
    {
        var baseFields = base.MergeGetFields();
        baseFields.Add("LeadMiningRequestId");
        return baseFields;
    }
}
