csharp
public partial class UtmCampaign {

    public void ComputeAbTestingCompleted()
    {
        this.AbTestingCompleted = this.AbTestingWinnerMailingId != null;
    }

    public void ComputeMailingMailCount()
    {
        var mailingData = Env.GetModel("MassMailing.Mailing").ReadGroup(
            new[] { ("CampaignId", "in", this.Id), ("MailingType", "=", "mail") },
            new[] { ("CampaignId"), ("AbTestingEnabled") },
            new[] { ("__count") }
        );
        var abTestingMappedData = new Dictionary<int, List<int>>();
        var mappedData = new Dictionary<int, List<int>>();
        foreach (var item in mailingData)
        {
            if ((bool)item["AbTestingEnabled"])
            {
                abTestingMappedData.AddOrUpdate((int)item["CampaignId"], new List<int>() { (int)item["__count"] }, (key, list) => list.Add((int)item["__count"]));
            }
            mappedData.AddOrUpdate((int)item["CampaignId"], new List<int>() { (int)item["__count"] }, (key, list) => list.Add((int)item["__count"]));
        }
        this.MailingMailCount = mappedData.ContainsKey(this.Id) ? mappedData[this.Id].Sum() : 0;
        this.AbTestingMailingsCount = abTestingMappedData.ContainsKey(this.Id) ? abTestingMappedData[this.Id].Sum() : 0;
    }

    public void ComputeStatistics()
    {
        var defaultVals = new Dictionary<string, object>()
        {
            {"ReceivedRatio", 0},
            {"OpenedRatio", 0},
            {"RepliedRatio", 0},
            {"BouncedRatio", 0}
        };
        if (this.Id == 0)
        {
            this.Update(defaultVals);
            return;
        }
        var allStats = Env.Cr.FetchAll($@"
            SELECT
                c.Id as CampaignId,
                COUNT(s.Id) AS Expected,
                COUNT(s.SentDatetime) AS Sent,
                COUNT(s.TraceStatus) FILTER (WHERE s.TraceStatus in ('sent', 'open', 'reply')) AS Delivered,
                COUNT(s.TraceStatus) FILTER (WHERE s.TraceStatus in ('open', 'reply')) AS Open,
                COUNT(s.TraceStatus) FILTER (WHERE s.TraceStatus = 'reply') AS Reply,
                COUNT(s.TraceStatus) FILTER (WHERE s.TraceStatus = 'bounce') AS Bounce,
                COUNT(s.TraceStatus) FILTER (WHERE s.TraceStatus = 'cancel') AS Cancel
            FROM
                MassMailing.MailingTrace s
            RIGHT JOIN
                MassMailing.UtmCampaign c
                ON (c.Id = s.CampaignId)
            WHERE
                c.Id IN ({string.Join(",", this.Id)})
            GROUP BY
                c.Id
        ");
        var statsPerCampaign = allStats.ToDictionary(stats => (int)stats["CampaignId"], stats => stats);
        var stats = statsPerCampaign.GetValueOrDefault(this.Id);
        if (stats == null)
        {
            this.Update(defaultVals);
        }
        else
        {
            var total = stats["Expected"] != null ? (int)stats["Expected"] - (int)stats["Cancel"] : 1;
            var delivered = stats["Sent"] != null ? (int)stats["Sent"] - (int)stats["Bounce"] : 0;
            var vals = new Dictionary<string, object>()
            {
                {"ReceivedRatio", Math.Round(100.0 * delivered / total, 2)},
                {"OpenedRatio", Math.Round(100.0 * stats["Open"] / total, 2)},
                {"RepliedRatio", Math.Round(100.0 * stats["Reply"] / total, 2)},
                {"BouncedRatio", Math.Round(100.0 * stats["Bounce"] / total, 2)}
            };
            this.Update(vals);
        }
    }

    public void ComputeIsMailingCampaignActivated()
    {
        this.IsMailingCampaignActivated = Env.User.HasGroup("MassMailing.GroupMassMailingCampaign");
    }

    public Dictionary<int, HashSet<int>> GetMailingRecipients(string model = null)
    {
        var res = new Dictionary<int, HashSet<int>>()
        {
            { this.Id, new HashSet<int>() }
        };
        var domain = new[] { ("CampaignId", "=", this.Id) };
        if (!string.IsNullOrEmpty(model))
        {
            domain = domain.Append(("Model", "=", model)).ToArray();
        }
        var mailingTraces = Env.GetModel("MassMailing.MailingTrace").Search(domain);
        res[this.Id] = new HashSet<int>(mailingTraces.Select(trace => trace.ResId).ToList());
        return res;
    }

    public void CronProcessMassMailingAbTesting()
    {
        var abTestingCampaign = this.Search(new[]
        {
            ("AbTestingScheduleDatetime", "<=", Env.GetNow()),
            ("AbTestingWinnerSelection", "!=", "Manual"),
            ("AbTestingCompleted", "=", false)
        });
        foreach (var campaign in abTestingCampaign)
        {
            var abTestingMailings = campaign.MailingMailIds.Where(m => m.AbTestingEnabled).ToList();
            if (!abTestingMailings.Any(m => m.State == "done"))
            {
                continue;
            }
            abTestingMailings.ForEach(m => m.ActionSendWinnerMailing());
        }
    }

}
