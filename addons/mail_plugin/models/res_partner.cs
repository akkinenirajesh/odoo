csharp
public partial class MailPlugin.ResPartner
{
    public void ComputePartnerIapInfo()
    {
        var partnerIaps = Env.Search<MailPlugin.ResPartnerIap>(new[] { new CSharp.Domain("PartnerId", CSharp.Condition.In, this.Id) });
        var partnerIapsPerPartner = partnerIaps.ToDictionary(partnerIap => partnerIap.PartnerId);
        this.IapEnrichInfo = partnerIapsPerPartner.TryGetValue(this.Id, out var partnerIap) ? partnerIap.IapEnrichInfo : false;
        this.IapSearchDomain = partnerIapsPerPartner.TryGetValue(this.Id, out partnerIap) ? partnerIap.IapSearchDomain : false;
    }

    public MailPlugin.ResPartner[] Create(MailPlugin.ResPartner[] valsList)
    {
        var partners = Env.Create<MailPlugin.ResPartner>(valsList);
        var partnerIapValsList = valsList.Where(vals => vals.IapEnrichInfo != null || vals.IapSearchDomain != null)
            .Select((vals, index) => new MailPlugin.ResPartnerIap
            {
                PartnerId = partners[index].Id,
                IapEnrichInfo = vals.IapEnrichInfo,
                IapSearchDomain = vals.IapSearchDomain
            }).ToArray();
        Env.Create<MailPlugin.ResPartnerIap>(partnerIapValsList);
        return partners;
    }

    public void Write(CSharp.Dictionary<string, object> vals)
    {
        Env.Write<MailPlugin.ResPartner>(this, vals);
        if (vals.ContainsKey("IapEnrichInfo") || vals.ContainsKey("IapSearchDomain"))
        {
            var partnerIaps = Env.Search<MailPlugin.ResPartnerIap>(new[] { new CSharp.Domain("PartnerId", CSharp.Condition.In, this.Id) });
            var missingPartners = new List<MailPlugin.ResPartner>(new[] { this });
            foreach (var partnerIap in partnerIaps)
            {
                if (vals.ContainsKey("IapEnrichInfo"))
                {
                    partnerIap.IapEnrichInfo = (string)vals["IapEnrichInfo"];
                }
                if (vals.ContainsKey("IapSearchDomain"))
                {
                    partnerIap.IapSearchDomain = (string)vals["IapSearchDomain"];
                }
                missingPartners.Remove(partnerIap.PartnerId);
            }
            if (missingPartners.Count > 0)
            {
                Env.Create<MailPlugin.ResPartnerIap>(missingPartners.Select(partner => new MailPlugin.ResPartnerIap
                {
                    PartnerId = partner.Id,
                    IapEnrichInfo = (string)vals.GetValueOrDefault("IapEnrichInfo"),
                    IapSearchDomain = (string)vals.GetValueOrDefault("IapSearchDomain")
                }).ToArray());
            }
        }
    }
}
