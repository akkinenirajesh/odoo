csharp
public partial class ChatbotScriptStep
{
    public Dictionary<string, object> ChatbotCrmPrepareLeadValues(DiscussChannel discussChannel, string description)
    {
        return new Dictionary<string, object>
        {
            ["Description"] = description + discussChannel.GetChannelHistory(),
            ["Name"] = $"{ChatbotScriptId.Title}'s New Lead",
            ["SourceId"] = ChatbotScriptId.SourceId.Id,
            ["TeamId"] = CrmTeamId.Id,
            ["Type"] = CrmTeamId.UseLeads ? "lead" : "opportunity",
            ["UserId"] = null
        };
    }

    public Message ProcessStep(DiscussChannel discussChannel)
    {
        var postedMessage = base.ProcessStep(discussChannel);

        if (StepType == "create_lead")
        {
            ProcessStepCreateLead(discussChannel);
        }

        return postedMessage;
    }

    private void ProcessStepCreateLead(DiscussChannel discussChannel)
    {
        var customerValues = ChatbotPrepareCustomerValues(discussChannel, createPartner: false, updatePartner: true);
        Dictionary<string, object> createValues;

        if (Env.User.IsPublic())
        {
            createValues = new Dictionary<string, object>
            {
                ["EmailFrom"] = customerValues["Email"],
                ["Phone"] = customerValues["Phone"]
            };
        }
        else
        {
            var partner = Env.User.PartnerId;
            createValues = new Dictionary<string, object>
            {
                ["PartnerId"] = partner.Id,
                ["CompanyId"] = partner.CompanyId.Id
            };
        }

        createValues.AddRange(ChatbotCrmPrepareLeadValues(discussChannel, customerValues["Description"].ToString()));

        Env.Get<CrmLead>().Create(createValues);
    }
}
