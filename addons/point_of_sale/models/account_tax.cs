csharp
public partial class AccountTax {

    public AccountTax Write(Dictionary<string, object> vals) {
        if (vals.ContainsKey("AmountType") || vals.ContainsKey("Amount") || vals.ContainsKey("TypeTaxUse") || vals.ContainsKey("TaxGroupId") || vals.ContainsKey("PriceInclude") || vals.ContainsKey("IncludeBaseAmount") || vals.ContainsKey("IsBaseAffected")) {
            var lines = Env.GetModel("Pos.OrderLine").Search(new Dictionary<string, object>() { { "OrderId.SessionId.State", "!=","closed" } });
            foreach (var linesChunk in lines.SplitEvery(100000)) {
                if (linesChunk.Any(l => l.TaxIds.Any(tid => this.Id == tid))) {
                    throw new Exception("It is forbidden to modify a tax used in a POS order not posted. You must close the POS sessions before modifying the tax.");
                }
                linesChunk.InvalidateRecordset("TaxIds");
            }
        }
        return (AccountTax)Env.GetModel("Account.AccountTax").Write(this.Id, vals);
    }

    public List<int> HookComputeIsUsed(List<int> taxesToCompute) {
        var usedTaxes = Env.GetModel("Account.AccountTax").HookComputeIsUsed(taxesToCompute);
        taxesToCompute.RemoveAll(t => usedTaxes.Contains(t));
        if (taxesToCompute.Count > 0) {
            Env.GetModel("Pos.OrderLine").FlushModel("TaxIds");
            var result = Env.Cr.Execute("SELECT id FROM account_tax WHERE EXISTS(SELECT 1 FROM account_tax_pos_order_line_rel AS pos WHERE account_tax_id IN @taxes AND account_tax.id = pos.account_tax_id)", new Dictionary<string, object>() { { "@taxes", taxesToCompute } });
            usedTaxes.AddRange(result.Select(r => (int)r[0]));
        }
        return usedTaxes;
    }

    public Dictionary<string, object> LoadPosData(Dictionary<string, object> data) {
        var domain = LoadPosDataDomain(data);
        var taxIds = Env.GetModel("Account.AccountTax").Search(domain);
        var taxesList = new List<Dictionary<string, object>>();
        foreach (var tax in taxIds) {
            taxesList.Add(tax.PrepareDictForTaxesComputation());
        }
        if (data.ContainsKey("pos.config") && ((List<Dictionary<string, object>>)data["pos.config"]["data"]).Count > 0) {
            var productFields = Env.GetModel("Account.AccountTax").EvalTaxesComputationPrepareProductFields(taxesList);
            ((List<Dictionary<string, object>>)data["pos.config"]["data"])[0]["_product_default_values"] = Env.GetModel("Account.AccountTax").EvalTaxesComputationPrepareProductDefaultValues(productFields);
        }
        return new Dictionary<string, object>() {
            { "data", taxesList },
            { "fields", LoadPosDataFields(((List<Dictionary<string, object>>)data["pos.config"]["data"])[0]["id"]) }
        };
    }

    public Dictionary<string, object> LoadPosDataDomain(Dictionary<string, object> data) {
        return Env.GetModel("Account.AccountTax").CheckCompanyDomain(data["pos.config"]["data"][0]["company_id"]);
    }

    public List<string> LoadPosDataFields(object configId) {
        return new List<string>() {
            "Id", "Name", "PriceInclude", "IncludeBaseAmount", "IsBaseAffected", "AmountType", "ChildrenTaxIds", "Amount", "RepartitionLineIds", "Company", "Id"
        };
    }

    public Dictionary<string, object> PrepareDictForTaxesComputation() {
        // Implementation for preparing tax dictionary 
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> CheckCompanyDomain(object companyId) {
        // Implementation for checking company domain
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> EvalTaxesComputationPrepareProductFields(List<Dictionary<string, object>> taxesList) {
        // Implementation for evaluating taxes computation
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> EvalTaxesComputationPrepareProductDefaultValues(Dictionary<string, object> productFields) {
        // Implementation for evaluating taxes computation
        return new Dictionary<string, object>();
    }
}
