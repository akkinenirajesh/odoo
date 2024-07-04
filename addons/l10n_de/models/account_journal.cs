csharp
public partial class AccountJournal
{
    public Dictionary<string, object> PrepareLiquidityAccountVals(Core.Company company, string code, Dictionary<string, object> vals)
    {
        var res = base.PrepareLiquidityAccountVals(company, code, vals);

        if (company.AccountFiscalCountry?.Code == "DE")
        {
            var tagIds = res.ContainsKey("TagIds") ? (List<int>)res["TagIds"] : new List<int>();
            var deAssetTag = Env.Ref<Account.AccountTag>("l10n_de.tag_de_asset_bs_B_IV");
            if (deAssetTag != null && !tagIds.Contains(deAssetTag.Id))
            {
                tagIds.Add(deAssetTag.Id);
            }
            res["TagIds"] = tagIds;
        }

        return res;
    }

    public override string ToString()
    {
        // Example implementation
        return Name;
    }
}
