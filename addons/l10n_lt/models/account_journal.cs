csharp
public partial class AccountJournal
{
    public Dictionary<string, object> PrepareLinquidityAccountVals(Core.Company company, string code, Dictionary<string, object> vals)
    {
        var accountVals = base.PrepareLinquidityAccountVals(company, code, vals);

        if (company.AccountFiscalCountry.Code == "LT")
        {
            if (!accountVals.ContainsKey("TagIds"))
            {
                accountVals["TagIds"] = new List<int>();
            }

            var tagIds = (List<int>)accountVals["TagIds"];
            var ltAccountTag = Env.Ref<Account.AccountTag>("l10n_lt.account_account_tag_b_4");
            tagIds.Add(ltAccountTag.Id);
        }

        return accountVals;
    }
}
