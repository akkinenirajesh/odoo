csharp
public partial class PosLoyalty_LoyaltyReward 
{
    public virtual List<object> GetDiscountProductValues()
    {
        var res = Env.Get("PosLoyalty.LoyaltyReward").Call<List<object>>("_GetDiscountProductValues");
        foreach (var vals in res)
        {
            ((Dictionary<string, object>)vals)["TaxesId"] = false;
        }
        return res;
    }

    public virtual List<object> LoadPosDataDomain(Dictionary<string, object> data)
    {
        var configId = Env.Get("Pos.Config").Browse<Pos_Config>(data["pos.config"]["data"][0]["id"]);
        return new List<object>() { new List<object>() { "ProgramId", "in", configId.Call<List<int>>("_GetProgramIds") } };
    }

    public virtual List<string> LoadPosDataFields(int configId)
    {
        return new List<string>() { "Description", "ProgramId", "RewardType", "RequiredPoints", "ClearWallet", "CurrencyId", "Discount", "DiscountMode", "DiscountApplicability", "AllDiscountProductIds", "IsGlobalDiscount", "DiscountMaxAmount", "DiscountLineProductId", "RewardProductId", "MultiProduct", "RewardProductIds", "RewardProductQty", "RewardProductUomId", "RewardProductDomain" };
    }

    public virtual Dictionary<string, object> LoadPosData(Dictionary<string, object> data)
    {
        var domain = LoadPosDataDomain(data);
        var fields = LoadPosDataFields((int)data["pos.config"]["data"][0]["id"]);
        var rewards = Env.Get("PosLoyalty.LoyaltyReward").SearchRead(domain, fields, false);
        foreach (var reward in rewards)
        {
            ((Dictionary<string, object>)reward)["RewardProductDomain"] = ReplaceIlikeWithIn(((Dictionary<string, object>)reward)["RewardProductDomain"]);
        }
        return new Dictionary<string, object>() { { "data", rewards }, { "fields", fields } };
    }

    public virtual List<string> GetRewardProductDomainFields(int configId)
    {
        var fields = new HashSet<string>();
        var config = Env.Get("Pos.Config").Browse<Pos_Config>(configId);
        var searchDomain = new List<object>() { new List<object>() { "ProgramId", "in", config.Call<List<int>>("_GetProgramIds") } };
        var domains = Env.Get("PosLoyalty.LoyaltyReward").SearchRead(searchDomain, new List<string>() { "RewardProductDomain" }, false);
        foreach (var domain in domains.Where(d => ((Dictionary<string, object>)d)["RewardProductDomain"] != "null"))
        {
            var domainObj = (List<object>)ast.LiteralEval(((Dictionary<string, object>)domain)["RewardProductDomain"]);
            foreach (var condition in ParseDomain(domainObj).Values)
            {
                var field_name = (string)condition[0];
                fields.Add(field_name);
            }
        }
        return fields.ToList();
    }

    public virtual string ReplaceIlikeWithIn(object domainStr)
    {
        if (domainStr.ToString() == "null")
        {
            return domainStr.ToString();
        }

        var domain = (List<object>)ast.LiteralEval(domainStr);

        for (var index = 0; index < domain.Count; index++)
        {
            var condition = (List<object>)domain[index];
            var field_name = (string)condition[0];
            var operator_ = (string)condition[1];
            var value = condition[2];
            var field = Env.Get("Product.Product").GetField(field_name);

            if (field != null && field.Type == "many2one" && (operator_ == "ilike" || operator_ == "not ilike"))
            {
                var comodel = Env.Get(field.CoModelName);
                var matching_ids = comodel.Call<List<int>>("_NameSearch", new object[] { value, new List<object>(), operator_, null });

                var newOperator = operator_ == "ilike" ? "in" : "not in";
                domain[index] = new List<object>() { field_name, newOperator, matching_ids };
            }
        }

        return json.Dumps(domain);
    }

    public virtual Dictionary<int, List<object>> ParseDomain(List<object> domain)
    {
        var parsedDomain = new Dictionary<int, List<object>>();

        for (var index = 0; index < domain.Count; index++)
        {
            var condition = (List<object>)domain[index];
            if (condition.Count == 3)
            {
                parsedDomain[index] = condition;
            }
        }

        return parsedDomain;
    }

    public virtual void Unlink()
    {
        if (this.Count == 1 && Env.Get("Pos.OrderLine").Sudo().SearchCount(new List<object>() { new List<object>() { "RewardId", "in", this.Ids } }, 1) > 0)
        {
            ActionArchive();
        }
        else
        {
            base.Unlink();
        }
    }
}
