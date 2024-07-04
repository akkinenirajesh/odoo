csharp
public partial class PaymentTransaction {

    public Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "paypal")
        {
            return processingValues;
        }

        var baseUrl = this.ProviderId.GetBaseUrl();
        var cancelUrl = urls.url_join(baseUrl, PaypalController.CancelUrl);
        var cancelUrlParams = new Dictionary<string, string>()
        {
            { "tx_ref", this.Reference },
            { "return_access_tkn", payment_utils.GenerateAccessToken(this.Reference) },
        };
        var partnerFirstName, partnerLastName = payment_utils.SplitPartnerName(this.PartnerName);
        return new Dictionary<string, object>()
        {
            { "address1", this.PartnerAddress },
            { "amount", this.Amount },
            { "business", this.ProviderId.PayPalEmailAccount },
            { "cancel_url", $"{cancelUrl}?{urls.url_encode(cancelUrlParams)}" },
            { "city", this.PartnerCity },
            { "country", this.PartnerCountryId.Code },
            { "currency_code", this.CurrencyId.Name },
            { "email", this.PartnerEmail },
            { "first_name", partnerFirstName },
            { "item_name", $"{this.CompanyId.Name}: {this.Reference}" },
            { "item_number", this.Reference },
            { "last_name", partnerLastName },
            { "lc", this.PartnerLang },
            { "notify_url", urls.url_join(baseUrl, PaypalController.WebhookUrl) },
            { "return_url", urls.url_join(baseUrl, PaypalController.ReturnUrl) },
            { "state", this.PartnerStateId.Name },
            { "zip_code", this.PartnerZip },
            { "api_url", this.ProviderId.GetPayPalApiUrl() },
        };
    }

    public PaymentTransaction GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "paypal")
        {
            return Env.GetModel("Payment.PaymentTransaction").Search(new Dictionary<string, object>() { { "reference", notificationData["item_number"] }, { "provider_code", "paypal" } }, 1);
        }
        return null;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "paypal")
        {
            return;
        }

        if (notificationData == null)
        {
            this.SetCanceled(new Dictionary<string, object>() { { "state_message", "The customer left the payment page." } });
            return;
        }

        var amount = notificationData.ContainsKey("amt") ? notificationData["amt"] : notificationData["mc_gross"];
        var currencyCode = notificationData.ContainsKey("cc") ? notificationData["cc"] : notificationData["mc_currency"];
        if (amount == null || currencyCode == null)
        {
            throw new ValidationError("PayPal: Missing amount or currency");
        }
        if (this.CurrencyId.CompareAmounts((float)amount, this.Amount) != 0)
        {
            throw new ValidationError("PayPal: Mismatching amounts");
        }
        if ((string)currencyCode != this.CurrencyId.Name)
        {
            throw new ValidationError("PayPal: Mismatching currency codes");
        }

        this.ProviderReference = (string)notificationData["txn_id"];
        this.PayPalType = (string)notificationData["txn_type"];

        this.PaymentMethodId = Env.GetModel("Payment.PaymentMethod").Search(new Dictionary<string, object>() { { "code", "paypal" } }, 1) ?? this.PaymentMethodId;

        var paymentStatus = (string)notificationData["payment_status"];

        if (PAYMENT_STATUS_MAPPING["pending"].Contains(paymentStatus))
        {
            this.SetPending(new Dictionary<string, object>() { { "state_message", notificationData["pending_reason"] } });
        }
        else if (PAYMENT_STATUS_MAPPING["done"].Contains(paymentStatus))
        {
            this.SetDone();
        }
        else if (PAYMENT_STATUS_MAPPING["cancel"].Contains(paymentStatus))
        {
            this.SetCanceled();
        }
        else
        {
            _logger.Info($"received data with invalid payment status ({paymentStatus}) for transaction with reference {this.Reference}");
            this.SetError("PayPal: Received data with invalid payment status: " + paymentStatus);
        }
    }
}
