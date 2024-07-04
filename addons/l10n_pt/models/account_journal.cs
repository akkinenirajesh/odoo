csharp
public partial class AccountJournal {
    public virtual AccountJournal PrepareLiquidityAccountVals(Company company, string code, Dictionary<string, object> vals) {
        var accountVals = Env.Call("Account.Journal", "PrepareLiquidityAccountVals", company, code, vals) as Dictionary<string, object>;
        if (company.AccountFiscalCountryId.Code == "PT") {
            if (vals.ContainsKey("Type") && vals["Type"].ToString() == "cash") {
                accountVals["L10nPtTaxonomyCode"] = 1;
            } else if (vals.ContainsKey("Type") && vals["Type"].ToString() == "bank") {
                accountVals["L10nPtTaxonomyCode"] = 2;
            }
        }
        return this;
    }
}
