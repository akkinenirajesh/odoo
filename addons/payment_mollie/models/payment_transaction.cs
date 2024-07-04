csharp
public partial class PaymentTransaction 
{
    public void GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "mollie")
        {
            return;
        }

        var payload = MolliePreparePaymentRequestPayload();
        // ... log using Env.Log
        var paymentData = this.AcquirerId.MollieMakeRequest("/payments", payload);
        this.ProviderReference = paymentData.Get("id");

        var checkoutUrl = paymentData.Get("_links").Get("checkout").Get("href");
        var parsedUrl = new Uri(checkoutUrl);
        var urlParams = new Dictionary<string, string>(HttpUtility.ParseQueryString(parsedUrl.Query));
        processingValues.Add("api_url", checkoutUrl);
        processingValues.Add("url_params", urlParams);
    }

    private Dictionary<string, object> MolliePreparePaymentRequestPayload()
    {
        var userLang = Env.Context.Get("lang");
        var baseUrl = this.AcquirerId.GetBaseUrl();
        var redirectUrl = new Uri(baseUrl).Combine(MollieController.ReturnUrl);
        var webhookUrl = new Uri(baseUrl).Combine(MollieController.WebhookUrl);

        return new Dictionary<string, object>
        {
            { "description", this.Reference },
            { "amount", new Dictionary<string, object>
                {
                    { "currency", this.CurrencyId.Name },
                    { "value", $"{this.Amount:0.00}" }
                }
            },
            { "locale", userLang != null && const.SUPPORTED_LOCALES.Contains(userLang) ? userLang : "en_US" },
            { "method", new List<string>
                {
                    const.PAYMENT_METHODS_MAPPING.GetOrDefault(this.PaymentMethodCode, this.PaymentMethodCode)
                }
            },
            { "redirectUrl", $"{redirectUrl}?ref={this.Reference}" },
            { "webhookUrl", $"{webhookUrl}?ref={this.Reference}" }
        };
    }

    public PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "mollie" || this.Id != default)
        {
            return this;
        }

        var tx = Env.Model("Payment.PaymentTransaction").Search([
            new Filter { Field = "Reference", Value = notificationData.Get("ref") },
            new Filter { Field = "ProviderCode", Value = "mollie" }
        ]);
        if (tx == null)
        {
            throw new Exception("Mollie: No transaction found matching reference " + notificationData.Get("ref"));
        }
        return tx;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "mollie")
        {
            return;
        }

        var paymentData = this.AcquirerId.MollieMakeRequest($"/payments/{this.ProviderReference}", "GET");

        var paymentMethodType = paymentData.Get("method");
        if (paymentMethodType == "creditcard")
        {
            paymentMethodType = paymentData.Get("details").Get("cardLabel").ToString().ToLower();
        }
        var paymentMethod = Env.Model("Payment.PaymentMethod").GetFromCode(paymentMethodType, const.PAYMENT_METHODS_MAPPING);
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;

        var paymentStatus = paymentData.Get("status");
        switch (paymentStatus)
        {
            case "pending":
                SetPending();
                break;
            case "authorized":
                SetAuthorized();
                break;
            case "paid":
                SetDone();
                break;
            case "expired":
            case "canceled":
            case "failed":
                SetCanceled("Mollie: Cancelled payment with status: " + paymentStatus);
                break;
            default:
                Env.Log($"received data with invalid payment status ({paymentStatus}) for transaction with reference {this.Reference}");
                SetError("Mollie: Received data with invalid payment status: " + paymentStatus);
                break;
        }
    }

    // ... rest of the methods (SetPending, SetAuthorized, SetDone, SetCanceled, SetError)
}
