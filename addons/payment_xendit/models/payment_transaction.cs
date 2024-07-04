C#
public partial class PaymentTransaction 
{
    public virtual void GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "xendit" || this.PaymentMethodCode != "card")
        {
            return;
        }
        var payload = this.PrepareInvoiceRequestPayload();
        var invoiceData = Env.Call("Payment.PaymentProvider", "XenditMakeRequest", "v2/invoices", payload);
        var apiUrl = invoiceData["invoice_url"];
        processingValues["api_url"] = apiUrl;
    }

    public virtual Dictionary<string, object> PrepareInvoiceRequestPayload()
    {
        var baseUrl = Env.Call("Payment.PaymentProvider", "GetBaseUrl");
        var redirectUrl = baseUrl + "/payment/status";
        var payload = new Dictionary<string, object>()
        {
            { "external_id", this.Reference },
            { "amount", this.Amount },
            { "description", this.Reference },
            { "customer", new Dictionary<string, object>()
                {
                    { "given_names", this.PartnerName },
                    { "email", this.PartnerEmail },
                    { "mobile_number", this.PartnerMobile },
                    { "addresses", new List<Dictionary<string, object>>()
                        {
                            new Dictionary<string, object>()
                            {
                                { "city", this.PartnerCity },
                                { "country", this.PartnerCountry.Name },
                                { "postal_code", this.PartnerZip },
                                { "state", this.PartnerState.Name },
                                { "street_line1", this.PartnerAddress }
                            }
                        }
                    }
                }
            },
            { "success_redirect_url", redirectUrl },
            { "failure_redirect_url", redirectUrl },
            { "payment_methods", new List<string>() { "card" } },
            { "currency", this.Currency.Name }
        };
        return payload;
    }

    public virtual void SendPaymentRequest()
    {
        if (this.ProviderCode != "xendit")
        {
            return;
        }
        if (this.Token == null)
        {
            throw new Exception("Xendit: The transaction is not linked to a token.");
        }
        this.CreateCharge(this.Token.ProviderReference);
    }

    public virtual void CreateCharge(string tokenRef)
    {
        var payload = new Dictionary<string, object>()
        {
            { "token_id", tokenRef },
            { "external_id", this.Reference },
            { "amount", this.Amount },
            { "currency", this.Currency.Name }
        };
        var chargeNotificationData = Env.Call("Payment.PaymentProvider", "XenditMakeRequest", "credit_card_charges", payload);
        this.HandleNotificationData("xendit", chargeNotificationData);
    }

    public virtual List<PaymentTransaction> GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "xendit")
        {
            return base.GetTransactionFromNotificationData(providerCode, notificationData);
        }
        var reference = notificationData["external_id"];
        if (reference == null)
        {
            throw new Exception("Xendit: Received data with missing reference.");
        }
        var tx = Env.Search<PaymentTransaction>(x => x.Reference == reference && x.ProviderCode == "xendit");
        if (tx.Count == 0)
        {
            throw new Exception(string.Format("Xendit: No transaction found matching reference {0}.", reference));
        }
        return tx;
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "xendit")
        {
            return;
        }
        this.ProviderReference = notificationData["id"];
        var paymentMethodCode = notificationData["payment_method"];
        var paymentMethod = Env.Call<Payment.PaymentMethod>("GetPaymentMethodFromCode", paymentMethodCode);
        this.PaymentMethod = paymentMethod;
        var paymentStatus = notificationData["status"];
        if (paymentStatus == "pending")
        {
            this.PaymentState = PaymentState.Pending;
        }
        else if (paymentStatus == "done")
        {
            this.TokenizeFromNotificationData(notificationData);
            this.PaymentState = PaymentState.Done;
        }
        else if (paymentStatus == "cancel")
        {
            this.PaymentState = PaymentState.Canceled;
        }
        else if (paymentStatus == "error")
        {
            var failureReason = notificationData["failure_reason"];
            this.PaymentState = PaymentState.Error;
        }
    }

    public virtual void TokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        var cardInfo = notificationData["masked_card_number"].ToString().Substring(notificationData["masked_card_number"].ToString().Length - 4);
        var tokenID = notificationData["credit_card_token_id"];
        var token = Env.Create<Payment.PaymentToken>(x =>
        {
            x.Provider = this.Provider;
            x.PaymentMethod = this.PaymentMethod;
            x.PaymentDetails = cardInfo;
            x.Partner = this.Partner;
            x.ProviderReference = tokenID;
        });
        this.Token = token;
    }

    public virtual void SetPending()
    {
        this.PaymentState = PaymentState.Pending;
    }

    public virtual void SetDone()
    {
        this.PaymentState = PaymentState.Done;
    }

    public virtual void SetCanceled()
    {
        this.PaymentState = PaymentState.Canceled;
    }

    public virtual void SetError(string failureReason)
    {
        this.PaymentState = PaymentState.Error;
    }
}
