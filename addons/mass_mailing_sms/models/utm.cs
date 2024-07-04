C#
public partial class MassMailingSms.UtmCampaign {

    public void ComputeMailingSmsCount() {
        var mailingSmsData = Env.Ref("MassMailing.Mailing").ReadGroup(new[] { new Condition("campaign_id", "in", this.Id), new Condition("MailingType", "=", "sms") }, new[] { "campaign_id", "AbTestingEnabled" }, new[] { "__count" });
        var abTestingMappedSmsData = new Dictionary<long, List<int>>();
        var mappedSmsData = new Dictionary<long, List<int>>();

        foreach (var data in mailingSmsData) {
            var campaign = data[0];
            var abTestingEnabled = data[1];
            var count = data[2];

            if (abTestingEnabled) {
                if (!abTestingMappedSmsData.ContainsKey(campaign)) {
                    abTestingMappedSmsData[campaign] = new List<int>();
                }
                abTestingMappedSmsData[campaign].Add(count);
            }

            if (!mappedSmsData.ContainsKey(campaign)) {
                mappedSmsData[campaign] = new List<int>();
            }
            mappedSmsData[campaign].Add(count);
        }

        this.MailingSmsCount = mappedSmsData[this.Id].Sum();
        this.AbTestingMailingsSmsCount = abTestingMappedSmsData[this.Id].Sum();
    }

    public Action ActionCreateMassSms() {
        var action = Env.Ref("MassMailing.action_create_mass_mailings_from_campaign");
        action.Context = new Dictionary<string, object>() {
            { "default_campaign_id", this.Id },
            { "default_mailing_type", "sms" },
            { "search_default_assigned_to_me", 1 },
            { "search_default_campaign_id", this.Id },
            { "default_user_id", Env.User.Id }
        };
        return action;
    }

    public Action ActionRedirectToMailingSms() {
        var action = Env.Ref("MassMailingSms.mailing_mailing_action_sms");
        action.Context = new Dictionary<string, object>() {
            { "default_campaign_id", this.Id },
            { "default_mailing_type", "sms" },
            { "search_default_assigned_to_me", 1 },
            { "search_default_campaign_id", this.Id },
            { "default_user_id", Env.User.Id }
        };
        action.Domain = new[] { new Condition("MailingType", "=", "sms") };
        return action;
    }

    public List<MassMailingSms.UtmCampaign> CronProcessMassMailingAbTesting() {
        var abTestingCampaign = base.CronProcessMassMailingAbTesting();
        foreach (var campaign in abTestingCampaign) {
            var abTestingMailings = campaign.MailingSmsIds.Where(m => m.AbTestingEnabled).ToList();
            if (!abTestingMailings.Any(m => m.State == "done")) {
                continue;
            }
            abTestingMailings.ForEach(m => m.SendWinnerMailing());
        }
        return abTestingCampaign;
    }
}

public partial class MassMailingSms.UtmMedium {

    public void UnlinkExceptUtmMediumSms() {
        var utmMediumSms = Env.Ref("mass_mailing_sms.utm_medium_sms", raiseIfNotFound: false);
        if (utmMediumSms != null && utmMediumSms == this) {
            throw new UserError(string.Format("The UTM medium '{0}' cannot be deleted as it is used in some main functional flows, such as the SMS Marketing.", utmMediumSms.Name));
        }
    }
}
