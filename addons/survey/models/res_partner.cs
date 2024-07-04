csharp
public partial class SurveyResPartner 
{
    public int CertificationsCount { get; set; }
    public int CertificationsCompanyCount { get; set; }

    public void ComputeCertificationsCount()
    {
        var readGroupRes = Env.Model("survey.user_input").Sudo().ReadGroup(
            new List<object> {
                new Dictionary<string, object> { { "PartnerId", new List<object> { this.Id } } },
                new Dictionary<string, object> { { "ScoringSuccess", true } }
            },
            new List<object> { "PartnerId" },
            new List<object> { "__count" }
        );

        var data = readGroupRes.Select(x => new { PartnerId = (int)x["PartnerId"], Count = (int)x["__count"] }).ToDictionary(x => x.PartnerId, x => x.Count);

        CertificationsCount = data.ContainsKey(this.Id) ? data[this.Id] : 0;
    }

    public void ComputeCertificationsCompanyCount()
    {
        CertificationsCompanyCount = Env.Model<SurveyResPartner>("Survey.ResPartner").Sudo().Search(new List<object> { new Dictionary<string, object> { { "ParentId", this.Id } } }).Sum(child => child.CertificationsCount);
    }

    public object ActionViewCertifications()
    {
        var action = Env.Model("ir.actions.actions")._ForXmlId("survey.res_partner_action_certifications");
        action["view_mode"] = "tree";
        action["domain"] = new List<object> { "OR", new Dictionary<string, object> { { "PartnerId", new List<object> { this.Id } } }, new Dictionary<string, object> { { "PartnerId", new List<object> { Env.Model<SurveyResPartner>("Survey.ResPartner").Sudo().Search(new List<object> { new Dictionary<string, object> { { "ParentId", this.Id } } }).Select(child => child.Id).ToList() } } } };
        return action;
    }
}
