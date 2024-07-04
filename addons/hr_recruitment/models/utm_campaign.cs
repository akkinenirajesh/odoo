csharp
public partial class UtmCampaign
{
    public void OnDelete()
    {
        var utmCampaignJob = Env.Ref<UtmCampaign>("Hr.Recruitment.UtmCampaignJob", raiseIfNotFound: false);
        if (utmCampaignJob != null && this.Id == utmCampaignJob.Id)
        {
            throw new UserException(
                $"The UTM campaign '{utmCampaignJob.Name}' cannot be deleted as it is used in the recruitment process."
            );
        }
    }
}
