csharp
public partial class AccountJournal
{
    public object PrepareLiquidityAccountVals(Core.Company company, string code, Dictionary<string, object> vals)
    {
        var accountVals = base.PrepareLiquidityAccountVals(company, code, vals);

        if (company.AccountFiscalCountry.Code == "DK")
        {
            // Ensure the newly liquidity accounts have the right account tag in order to be part
            // of the Danish financial reports.
            if (!accountVals.ContainsKey("TagIds"))
            {
                accountVals["TagIds"] = new List<int>();
            }

            if (vals.ContainsKey("Type"))
            {
                if (vals["Type"].ToString() == "bank")
                {
                    ((List<int>)accountVals["TagIds"]).Add(Env.Ref<Account.AccountTag>("l10n_dk.account_tag_6481").Id);
                }
                else if (vals["Type"].ToString() == "cash")
                {
                    ((List<int>)accountVals["TagIds"]).Add(Env.Ref<Account.AccountTag>("l10n_dk.account_tag_6471").Id);
                }
            }
        }

        return accountVals;
    }
}
