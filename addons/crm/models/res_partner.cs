csharp
public partial class Partner
{
    public override IDictionary<string, object> DefaultGet(IEnumerable<string> fields)
    {
        var rec = base.DefaultGet(fields);
        var activeModel = Env.Context.GetValueOrDefault("active_model") as string;
        if (activeModel == "Crm.Lead" && Env.Context.GetValueOrDefault("active_ids") is List<int> activeIds && activeIds.Count <= 1)
        {
            var lead = Env.Get<Lead>().Browse(Env.Context.GetValueOrDefault("active_id") as int?).FirstOrDefault();
            if (lead != null)
            {
                rec.Update(new Dictionary<string, object>
                {
                    { "Phone", lead.Phone },
                    { "Mobile", lead.Mobile },
                    { "Function", lead.Function },
                    { "Title", lead.Title?.Id },
                    { "Website", lead.Website },
                    { "Street", lead.Street },
                    { "Street2", lead.Street2 },
                    { "City", lead.City },
                    { "StateId", lead.StateId?.Id },
                    { "CountryId", lead.CountryId?.Id },
                    { "Zip", lead.Zip }
                });
            }
        }
        return rec;
    }

    public void ComputeOpportunityCount()
    {
        var allPartners = Env.Get<Partner>().WithContext(new { active_test = false })
            .SearchFetch(new List<object> { new List<object> { "Id", "child_of", this.Id } }, new List<string> { "ParentId" });

        var opportunityData = Env.Get<Lead>().WithContext(new { active_test = false })
            .ReadGroup(
                domain: new List<object> { new List<object> { "PartnerId", "in", allPartners.Select(p => p.Id).ToList() } },
                groupby: new List<string> { "PartnerId" },
                aggregates: new List<string> { "__count" }
            );

        this.OpportunityCount = 0;
        foreach (var (partner, count) in opportunityData)
        {
            var currentPartner = partner as Partner;
            while (currentPartner != null)
            {
                if (currentPartner.Id == this.Id)
                {
                    this.OpportunityCount += (int)count;
                }
                currentPartner = currentPartner.ParentId;
            }
        }
    }

    public ActionResult ActionViewOpportunity()
    {
        var action = Env.Get<IrActionsActWindow>().ForXmlId("crm.crm_lead_opportunities");
        action.Context = new Dictionary<string, object>();
        
        if (this.IsCompany)
        {
            action.Domain = new List<object> { new List<object> { "PartnerId.CommercialPartnerId", "=", this.Id } };
        }
        else
        {
            action.Domain = new List<object> { new List<object> { "PartnerId", "=", this.Id } };
        }

        action.Domain = Expression.AND(action.Domain, new List<object> { new List<object> { "Active", "in", new List<object> { true, false } } });
        
        return action;
    }
}
