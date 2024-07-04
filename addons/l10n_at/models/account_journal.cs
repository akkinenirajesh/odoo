csharp
public partial class AccountJournal
{
    public Dictionary<string, object> PrepareLiquidityAccountVals(Core.Company company, string code, Dictionary<string, object> vals)
    {
        var accountVals = base.PrepareLiquidityAccountVals(company, code, vals);

        if (company.AccountFiscalCountry?.Code == "AT")
        {
            if (!accountVals.ContainsKey("TagIds"))
            {
                accountVals["TagIds"] = new List<int>();
            }

            var tagIds = (List<int>)accountVals["TagIds"];
            tagIds.Add(Env.Ref<Account.AccountTag>("l10n_at.account_tag_l10n_at_ABIV").Id);
            tagIds.Add(Env.Ref<Account.AccountTag>("l10n_at.account_tag_external_code_2300").Id);
        }

        return accountVals;
    }
}
