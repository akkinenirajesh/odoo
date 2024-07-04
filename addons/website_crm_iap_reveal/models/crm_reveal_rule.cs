csharp
public partial class CRMRevealRule 
{
    public virtual int GetLeadCount() 
    {
        var leadCount = Env.GetModel("crm.lead").SearchRead(new List<object> { new object[] { "reveal_rule_id", "=", this.Id }, new object[] { "type", "=", "lead" } }, new List<string> { "id" });
        return leadCount.Count;
    }

    public virtual int GetOpportunityCount() 
    {
        var opportunityCount = Env.GetModel("crm.lead").SearchRead(new List<object> { new object[] { "reveal_rule_id", "=", this.Id }, new object[] { "type", "=", "opportunity" } }, new List<string> { "id" });
        return opportunityCount.Count;
    }

    public virtual void ProcessLeadGeneration(bool autocommit = true) 
    {
        Env.GetModel("crm.reveal.view").CleanRevealViews();
        UnlinkUnrelevantRevealView();
        var revealViews = GetRevealViewsToProcess();
        int viewCount = 0;
        while (revealViews.Count > 0) 
        {
            viewCount += revealViews.Count;
            var serverPayload = PrepareIapPayload(revealViews);
            var enoughCredit = PerformRevealService(serverPayload);
            if (autocommit)
            {
                Env.Cr.Commit();
            }
            if (enoughCredit)
            {
                revealViews = GetRevealViewsToProcess();
            } 
            else 
            {
                revealViews = new List<object>();
            }
        }
    }

    public virtual void UnlinkUnrelevantRevealView() 
    {
        int monthsValid = Convert.ToInt32(Env.GetConfigParameter("reveal.lead_month_valid", 6));
        var leads = Env.GetModel("crm.lead").SearchRead(new List<object> { new object[] { "reveal_ip", "!=", null }, new object[] { "create_date", ">", DateTime.Now.AddMonths(-monthsValid) } }, new List<string> { "reveal_ip" });
        var revealViews = Env.GetModel("crm.reveal.view").SearchRead(new List<object> { new object[] { "reveal_ip", "in", leads.Select(l => l["reveal_ip"]) } }, new List<string> { "id" });
        revealViews.ForEach(v => Env.GetModel("crm.reveal.view").Unlink(new List<object> { v["id"] }));
    }

    public virtual List<object> GetRevealViewsToProcess() 
    {
        var query = @"
            SELECT v.reveal_ip, array_agg(v.reveal_rule_id ORDER BY r.sequence)
            FROM crm_reveal_view v
            INNER JOIN crm_reveal_rule r
            ON v.reveal_rule_id = r.id
            WHERE v.reveal_state='to_process'
            GROUP BY v.reveal_ip
            LIMIT 25
        ";
        Env.Cr.Execute(query);
        return Env.Cr.Fetchall();
    }

    public virtual Dictionary<string, object> PrepareIapPayload(List<object> pgv) 
    {
        var newIds = pgv.SelectMany(p => (List<object>)p[1]).Distinct().ToList();
        var ruleRecords = Env.GetModel("WebsiteCrmIapReveal.CRMRevealRule").Browse(newIds);
        return new Dictionary<string, object>()
        {
            { "ips", pgv.ToDictionary(p => (string)p[0], p => (List<object>)p[1]) },
            { "rules", ruleRecords.GetRulesPayload() }
        };
    }

    public virtual Dictionary<string, object> GetRulesPayload() 
    {
        var companyCountry = Env.Company.Country;
        var rulePayload = new Dictionary<string, object>();
        foreach (var rule in this) 
        {
            var revealIds = rule.Industries.SelectMany(i => i.RevealIds.Split(',')).Select(i => i.Trim()).ToList();
            var data = new Dictionary<string, object>()
            {
                { "rule_id", rule.Id },
                { "lead_for", rule.DataTracking },
                { "countries", rule.Countries.Select(c => c.Code).ToList() },
                { "filter_on_size", rule.FilterOnSize },
                { "company_size_min", rule.CompanySizeMin },
                { "company_size_max", rule.CompanySizeMax },
                { "industry_tags", revealIds },
                { "user_country", companyCountry != null ? companyCountry.Code : null }
            };
            if (rule.DataTracking == "People")
            {
                data.Add("contact_filter_type", rule.ContactFilterType);
                data.Add("preferred_role", rule.PreferredRole.RevealId ?? "");
                data.Add("other_roles", rule.OtherRoles.Select(r => r.RevealId).ToList());
                data.Add("seniority", rule.Seniority.RevealId ?? "");
                data.Add("extra_contacts", rule.NumberOfContacts - 1);
            }
            rulePayload.Add(rule.Id, data);
        }
        return rulePayload;
    }

    public virtual bool PerformRevealService(Dictionary<string, object> serverPayload) 
    {
        var accountToken = Env.GetModel("iap.account").Get("reveal");
        var params = new Dictionary<string, object>()
        {
            { "account_token", accountToken.AccountToken },
            { "data", serverPayload }
        };
        var result = IapContactReveal(params, 300);

        var allIps = serverPayload["ips"].Keys.ToList();
        var doneIps = new List<string>();
        foreach (var res in result.Get("reveal_data") as List<object>) 
        {
            doneIps.Add((string)res["ip"]);
            if (!(bool)res.Get("not_found")) 
            {
                var lead = CreateLeadFromResponse(res);
                Env.GetModel("crm.reveal.view").Unlink(new List<object> { Env.GetModel("crm.reveal.view").SearchRead(new List<object> { new object[] { "reveal_ip", "=", res["ip"] } }, new List<string> { "id" })[0]["id"] });
            } 
            else 
            {
                var views = Env.GetModel("crm.reveal.view").SearchRead(new List<object> { new object[] { "reveal_ip", "=", res["ip"] } }, new List<string> { "id" });
                views.ForEach(v => Env.GetModel("crm.reveal.view").Write(new List<object> { v["id"], new Dictionary<string, object>() { { "reveal_state", "not_found" } } }));
            }
        }

        if ((bool)result.Get("credit_error")) 
        {
            Env.GetModel("crm.iap.lead.helpers").NotifyNoMoreCredit("reveal", this.GetType().Name, "reveal.already_notified");
            return false;
        }
        else 
        {
            var views = Env.GetModel("crm.reveal.view").SearchRead(new List<object> { new object[] { "reveal_ip", "in", allIps.Except(doneIps) } }, new List<string> { "id" });
            views.ForEach(v => Env.GetModel("crm.reveal.view").Write(new List<object> { v["id"], new Dictionary<string, object>() { { "reveal_state", "not_found" } } }));
            Env.GetConfigParameter().SetParam("reveal.already_notified", false);
        }
        return true;
    }

    public virtual object IapContactReveal(Dictionary<string, object> params, int timeout)
    {
        var endpoint = Env.GetConfigParameter("reveal.endpoint", "https://iap-services.odoo.com") + "/iap/clearbit/1/reveal";
        return Env.GetModel("iap.tools").IapJsonrpc(endpoint, params, timeout);
    }

    public virtual object CreateLeadFromResponse(object result) 
    {
        var rule = Env.GetModel("WebsiteCrmIapReveal.CRMRevealRule").Browse(result["rule_id"]);
        if (rule == null)
        {
            return null;
        }
        if (result["clearbit_id"] == null) 
        {
            return null;
        }
        var alreadyCreatedLead = Env.GetModel("crm.lead").SearchRead(new List<object> { new object[] { "reveal_id", "=", result["clearbit_id"] } }, new List<string> { "id" });
        if (alreadyCreatedLead.Count > 0)
        {
            return null;
        }
        var leadVals = rule.LeadValsFromResponse(result);

        var lead = Env.GetModel("crm.lead").Create(leadVals);

        var templateValues = result["reveal_data"] as Dictionary<string, object>;
        templateValues.Add("flavor_text", "Opportunity created by Odoo Lead Generation");
        templateValues.Add("people_data", result.Get("people_data"));

        lead.MessagePostWithSource("iap_mail.enrich_company", templateValues, "mail.mt_note");

        return lead;
    }

    public virtual Dictionary<string, object> LeadValsFromResponse(object result) 
    {
        var companyData = result["reveal_data"] as Dictionary<string, object>;
        var peopleData = result.Get("people_data") as Dictionary<string, object>;
        var leadVals = Env.GetModel("crm.iap.lead.helpers").LeadValsFromResponse(this.Type, this.SalesTeam.Id, this.Tags.Select(t => t.Id).ToList(), this.Salesperson.Id, companyData, peopleData);

        leadVals.Add("Priority", this.Priority);
        leadVals.Add("reveal_ip", result["ip"]);
        leadVals.Add("reveal_rule_id", this.Id);
        leadVals.Add("referred", "Website Visitor");
        leadVals.Add("reveal_iap_credits", result["credit"]);

        if (this.Suffix != null)
        {
            leadVals["name"] = $"{leadVals["name"]} - {this.Suffix}";
        }

        return leadVals;
    }
}
