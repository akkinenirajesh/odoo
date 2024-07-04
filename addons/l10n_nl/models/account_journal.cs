csharp
public partial class AccountJournal 
{
    public virtual AccountJournal _prepare_liquidity_account_vals(Company company, string code, Dictionary<string, object> vals)
    {
        AccountJournal accountJournal = Env.Call("super", "_prepare_liquidity_account_vals", company, code, vals) as AccountJournal;
        if (company.AccountFiscalCountryId.Code == "NL")
        {
            accountJournal.TagIds.Add(Env.Ref("l10n_nl.account_tag_25"));
        }

        return accountJournal;
    }
}
