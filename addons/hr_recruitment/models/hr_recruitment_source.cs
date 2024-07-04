csharp
public partial class RecruitmentSource
{
    public string ComputeHasDomain()
    {
        if (Alias != null)
        {
            return Alias.AliasDomainId != null ? "true" : "false";
        }
        else
        {
            return (Job.Company.AliasDomainId != null || Env.Company.AliasDomainId != null) ? "true" : "false";
        }
    }

    public void CreateAlias()
    {
        var campaign = Env.Ref("hr_recruitment.utm_campaign_job");
        var medium = Env.UtmMedium.FetchOrCreateUtmMedium("email");

        if (Alias == null)
        {
            var aliasDefaults = new Dictionary<string, object>
            {
                {"JobId", Job.Id},
                {"CampaignId", campaign.Id},
                {"MediumId", medium.Id},
                {"SourceId", SourceId}
            };

            var aliasVals = new Dictionary<string, object>
            {
                {"AliasDefaults", aliasDefaults},
                {"AliasDomainId", Job.Company.AliasDomainId?.Id ?? Env.Company.AliasDomainId.Id},
                {"AliasModelId", Env.IrModel.GetId("Hr.Applicant")},
                {"AliasName", $"{Job.AliasName ?? Job.Name}+{Name}"},
                {"AliasParentThreadId", Job.Id},
                {"AliasParentModelId", Env.IrModel.GetId("Hr.Job")}
            };

            // Check access rights
            CheckAccessRights("create");
            CheckAccessRule("create");

            Alias = Env.MailAlias.Sudo().Create(aliasVals);
        }
    }

    public override bool Unlink()
    {
        var aliases = Alias;
        var result = base.Unlink();
        aliases.Sudo().Unlink();
        return result;
    }
}
