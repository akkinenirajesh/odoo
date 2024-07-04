csharp
public partial class CRMRevealView
{
    public CRMRevealView()
    {
    }

    public void CleanRevealViews()
    {
        int weeksValid = int.Parse(Env.ConfigParameter.GetParam("reveal.view_weeks_valid", DEFAULT_REVEAL_VIEW_WEEKS_VALID));
        DateTime today = DateTime.Now.Date;
        DateTime validDate = today.AddDays(-weeksValid * 7);

        var domain = new List<object[]>()
        {
            new object[] { "RevealState", "=", "NotFound" },
            new object[] { "CreateDate", "<", validDate }
        };
        var views = Env.Model("WebsiteCrmIapReveal.CRMRevealView").Search(domain);
        views.Unlink();
    }

    public List<string> CreateRevealView(long websiteId, string url, string ipAddress, string countryCode, string stateCode, List<string> rulesExcluded)
    {
        var rules = Env.Model("WebsiteCrmIapReveal.CRMRevealRule").MatchUrl(websiteId, url, countryCode, stateCode, rulesExcluded);
        if (rules.Count > 0)
        {
            var query = string.Format("INSERT INTO crm_reveal_view (reveal_ip, reveal_rule_id, reveal_state, create_date) VALUES ({0}, {1}, 'to_process', now() at time zone 'UTC') ON CONFLICT DO NOTHING", "reveal_ip", "reveal_rule_id");
            var paramsList = new List<object[]>();
            foreach (var rule in rules)
            {
                paramsList.Add(new object[] { ipAddress, rule["id"] });
                rulesExcluded.Add(rule["id"].ToString());
            }
            Env.Cr.Execute(query, paramsList);
            return rulesExcluded;
        }
        return null;
    }

    private const int DEFAULT_REVEAL_VIEW_WEEKS_VALID = 5;
}
