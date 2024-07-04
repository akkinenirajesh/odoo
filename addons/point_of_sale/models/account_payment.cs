csharp
public partial class AccountPayment 
{
    public AccountPayment()
    {

    }

    public virtual Account.Account GetValidLiquidityAccounts()
    {
        var result = Env.CallMethod("Account.Payment", "_get_valid_liquidity_accounts");
        return result | this.PosPaymentMethodId.OutstandingAccountId;
    }

    public virtual void ComputeOutstandingAccountId()
    {
        Env.CallMethod("Account.Payment", "_compute_outstanding_account_id");
        if (this.ForceOutstandingAccountId != null)
        {
            this.OutstandingAccountId = this.ForceOutstandingAccountId;
        }
    }
}
