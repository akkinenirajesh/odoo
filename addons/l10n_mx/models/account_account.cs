csharp
public partial class Account
{
    public override void OnCreate()
    {
        base.OnCreate();

        var debitTag = Env.Ref<Account.AccountTag>("l10n_mx.tag_debit_balance_account");
        var creditTag = Env.Ref<Account.AccountTag>("l10n_mx.tag_credit_balance_account");

        if (Company.Country.Code == "MX" && !Tags.Contains(debitTag) && !Tags.Contains(creditTag))
        {
            var DEBIT_CODES = new[] { '1', '5', '6', '7' };
            var tagToAdd = DEBIT_CODES.Contains(Code[0]) ? debitTag : creditTag;
            Tags.Add(tagToAdd);
        }
    }
}
