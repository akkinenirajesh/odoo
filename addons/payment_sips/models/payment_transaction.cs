csharp
public partial class PaymentTransaction 
{
    public string ComputeReference(string providerCode, string prefix, string separator)
    {
        if (providerCode == "sips")
        {
            prefix = Env.Ref("payment.singularize_reference_prefix", separator: "");
            separator = "x";
        }
        return Env.Call<string>("payment.transaction", "ComputeReference", providerCode, prefix, separator);
    }

    public Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "sips")
        {
            return Env.Call<Dictionary<string, object>>("payment.transaction", "GetSpecificRenderingValues", processingValues);
        }
        string baseUrl = Env.Ref("base.url");
        string apiUrl = this.ProviderId.State == "enabled" ? this.ProviderId.SipsProdUrl : this.ProviderId.SipsTestUrl;
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "amount", Env.Call<decimal>("payment.utils", "ToMinorCurrencyUnits", this.Amount, this.CurrencyId.Name) },
            { "currencyCode", Env.Ref("payment_sips.SUPPORTED_CURRENCIES", this.CurrencyId.Name) },
            { "merchantId", this.ProviderId.SipsMerchantId },
            { "normalReturnUrl", Env.Call<string>("payment.sips.controllers.main", "ReturnUrl", baseUrl) },
            { "automaticResponseUrl", Env.Call<string>("payment.sips.controllers.main", "WebhookUrl", baseUrl) },
            { "transactionReference", this.Reference },
            { "statementReference", this.Reference },
            { "keyVersion", this.ProviderId.SipsKeyVersion },
            { "returnContext", Env.Call<string>("json", "Dumps", new Dictionary<string, object> { { "reference", this.Reference } }) },
        };
        string dataString = string.Join("|", data.Select(x => $"{x.Key}={x.Value}"));
        return new Dictionary<string, object>()
        {
            { "apiUrl", apiUrl },
            { "Data", dataString },
            { "InterfaceVersion", this.ProviderId.SipsVersion },
            { "Seal", Env.Call<string>("payment.sips.controllers.main", "_sips_generate_shasign", dataString) },
        };
    }

    public PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        PaymentTransaction tx = Env.Call<PaymentTransaction>("payment.transaction", "GetTxFromNotificationData", providerCode, notificationData);
        if (providerCode != "sips" || tx != null)
        {
            return tx;
        }
        Dictionary<string, object> data = this._SipsNotificationDataToObject(notificationData["Data"].ToString());
        string reference = data.GetValueOrDefault("transactionReference").ToString();
        if (string.IsNullOrEmpty(reference))
        {
            Dictionary<string, object> returnContext = Env.Call<Dictionary<string, object>>("json", "Loads", data.GetValueOrDefault("returnContext").ToString());
            reference = returnContext.GetValueOrDefault("reference").ToString();
        }
        tx = Env.Search<PaymentTransaction>(x => x.Reference == reference && x.ProviderCode == "sips");
        if (tx == null)
        {
            throw new Exception($"Sips: No transaction found matching reference {reference}.");
        }
        return tx;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        Env.Call<PaymentTransaction>("payment.transaction", "ProcessNotificationData", notificationData);
        if (this.ProviderCode != "sips")
        {
            return;
        }
        Dictionary<string, object> data = this._SipsNotificationDataToObject(notificationData.GetValueOrDefault("Data").ToString());
        this.ProviderReference = data.GetValueOrDefault("transactionReference").ToString();
        string paymentMethodType = notificationData.GetValueOrDefault("paymentMeanBrand").ToString().ToLower();
        PaymentMethod paymentMethod = Env.Ref<PaymentMethod>(paymentMethodType);
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;
        string responseCode = data.GetValueOrDefault("responseCode").ToString();
        if (Env.Call<bool>("payment_sips.const", "RESPONSE_CODES_MAPPING", "pending", responseCode))
        {
            this.State = "pending";
        }
        else if (Env.Call<bool>("payment_sips.const", "RESPONSE_CODES_MAPPING", "done", responseCode))
        {
            this.State = "done";
        }
        else if (Env.Call<bool>("payment_sips.const", "RESPONSE_CODES_MAPPING", "cancel", responseCode))
        {
            this.State = "cancel";
        }
        else
        {
            this.State = "error";
        }
    }

    private Dictionary<string, object> _SipsNotificationDataToObject(string data)
    {
        Dictionary<string, object> res = new Dictionary<string, object>();
        foreach (string element in data.Split('|'))
        {
            string[] parts = element.Split('=', 2);
            res[parts[0]] = parts[1];
        }
        return res;
    }
}
