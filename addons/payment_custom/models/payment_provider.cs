csharp
public partial class PaymentProvider
{
    public void RecomputePendingMessage()
    {
        var accountPaymentModule = Env.Ref<IrModuleModule>("account_payment");
        if (accountPaymentModule.State == "installed")
        {
            if (this.CustomMode == "wire_transfer")
            {
                var companyID = this.CompanyID;
                var accounts = Env.Search<AccountJournal>(new[] {
                    Env.CheckCompanyDomain(companyID),
                    new Domain("Type", "=", "bank"),
                }).BankAccountID;
                var accountNames = string.Join("", accounts.Select(account => $"<li><pre>{account.DisplayName}</pre></li>"));
                this.PendingMessage = $"<div>"
                    + $"<h5>{Env.Translate("Please use the following transfer details")}</h5>"
                    + $"<p><br></p>"
                    + $"<h6>{Env.Translate("Bank Account") if accounts.Count == 1 else Env.Translate("Bank Accounts")}</h6>"
                    + $"<ul>{accountNames}</ul>"
                    + $"<p><br></p>"
                    + $"</div>";
            }
        }
    }

    public Domain GetRemovalDomain(string providerCode, string customMode = "")
    {
        var res = Env.Call<Domain>("_get_removal_domain", this, providerCode, customMode);
        if (providerCode == "custom" && !string.IsNullOrEmpty(customMode))
        {
            return res.And(new Domain("CustomMode", "=", customMode));
        }
        return res;
    }

    public Dictionary<string, object> GetRemovalValues()
    {
        var res = Env.Call<Dictionary<string, object>>("_get_removal_values", this);
        res["CustomMode"] = null;
        return res;
    }

    public void EnsurePendingMessageIsSet()
    {
        if (this.CustomMode == "wire_transfer" && string.IsNullOrEmpty(this.PendingMessage))
        {
            this.RecomputePendingMessage();
        }
    }

    public List<string> GetDefaultPaymentMethodCodes()
    {
        var defaultCodes = Env.Call<List<string>>("_get_default_payment_method_codes", this);
        if (this.Code != "custom" || this.CustomMode != "wire_transfer")
        {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
