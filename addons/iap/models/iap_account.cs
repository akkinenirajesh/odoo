csharp
public partial class IapAccount
{
    public override string ToString()
    {
        return ServiceName;
    }

    public void ComputeInfo()
    {
        if (AccountInfos.Any())
        {
            AccountInfo = AccountInfos.Last();
        }
    }

    public void ComputeBalance()
    {
        Balance = AccountInfo != null ? $"{AccountInfo.Balance} {AccountInfo.UnitName}" : "0 Credits";
    }

    public void InverseInfo()
    {
        if (AccountInfos.Any())
        {
            var accountInfo = Env.Get<Iap.IapAccountInfo>().Browse(AccountInfos.First().Id);
            accountInfo.Account = null;
        }
        AccountInfo.Account = this;
    }

    public List<int> SearchInfo(string @operator, object value)
    {
        return new List<int>();
    }

    public void Write(Dictionary<string, object> values)
    {
        base.Write(values);

        string[] iapEdits = { "WarnMe", "WarningThreshold", "WarningEmail" };
        if (iapEdits.Any(attr => values.ContainsKey(attr)))
        {
            try
            {
                string route = "/iap/update-warning-odoo";
                string endpoint = IapTools.IapGetEndpoint(Env);
                string url = endpoint + route;
                var data = new Dictionary<string, object>
                {
                    { "account_token", AccountToken },
                    { "dbuuid", Env.Get<Core.IrConfigParameter>().Sudo().GetParam("database.uuid") },
                    { "warn_me", values.GetValueOrDefault("WarnMe") },
                    { "warning_threshold", values.GetValueOrDefault("WarningThreshold") },
                    { "warning_email", values.GetValueOrDefault("WarningEmail") }
                };
                IapTools.IapJsonrpc(url, data);
            }
            catch (AccessError e)
            {
                Env.Logger.Warning($"Save service error : {e}");
            }
        }
    }

    public void GetServices()
    {
        // Implementation of GetServices method
    }

    public IapAccount Get(string serviceName, bool forceCreate = true)
    {
        // Implementation of Get method
    }

    public string GetCreditsUrl(string serviceName, string baseUrl = "", int credit = 0, bool trial = false, string accountToken = null)
    {
        // Implementation of GetCreditsUrl method
    }

    public Dictionary<string, object> ActionBuyCredits()
    {
        // Implementation of ActionBuyCredits method
    }

    public void ActionToggleShowToken()
    {
        ShowToken = !ShowToken;
    }

    public string GetConfigAccountUrl()
    {
        // Implementation of GetConfigAccountUrl method
    }

    public float GetCredits(string serviceName)
    {
        // Implementation of GetCredits method
    }
}
