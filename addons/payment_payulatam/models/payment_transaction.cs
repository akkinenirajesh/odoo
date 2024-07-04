csharp
public partial class PaymentTransaction 
{
    public virtual string ProviderReference { get; set; }
    public virtual string ProviderCode { get; set; }
    public virtual string PaymentToken { get; set; }
    public virtual string PaymentMethodCode { get; set; }
    public virtual string Reference { get; set; }
    public virtual Core.Currency CurrencyId { get; set; }
    public virtual double Amount { get; set; }
    public virtual string PartnerName { get; set; }
    public virtual string PartnerEmail { get; set; }
    public virtual Payment.PaymentMethod PaymentMethodId { get; set; }
    public virtual Payment.TransactionState State { get; set; }
    public virtual Payment.Provider ProviderId { get; set; }
    public virtual DateTime Date { get; set; }
    public virtual string AcquirerReference { get; set; }
    public virtual int TransactionId { get; set; }
    public virtual string Operation { get; set; }
    public virtual double Fees { get; set; }
    public virtual DateTime DateValid { get; set; }
    public virtual Res.Partner PartnerId { get; set; }
    public virtual DateTime PaymentDate { get; set; }
    public virtual int OrderId { get; set; }
    public virtual DateTime DateCreated { get; set; }
    public virtual Payment.Token PaymentTokenId { get; set; }
    public virtual string Signature { get; set; }
    public virtual int PaymentTransactionId { get; set; }
    public virtual DateTime LastSuccessDate { get; set; }
    public virtual DateTime LastErrorDate { get; set; }
    public virtual string LastErrorDescription { get; set; }
    public virtual string LastErrorMessage { get; set; }
    public virtual DateTime LastStateChange { get; set; }
    public virtual string LastSuccessMessage { get; set; }
    public virtual string LastSuccessDescription { get; set; }
    public virtual DateTime LastTransactionDate { get; set; }
    public virtual string LastTransactionMessage { get; set; }
    public virtual string LastTransactionDescription { get; set; }
    public virtual int LastTransactionErrorCode { get; set; }

    public virtual string ComputeReference(string providerCode, string prefix = null, string separator = "-", params object[] args) 
    { 
        if (providerCode == "payulatam") 
        {
            if (prefix == null) 
            {
                prefix = this.ComputeReferencePrefix(providerCode, separator, args);
            }
            prefix = PaymentUtils.SingularizeReferencePrefix(prefix, separator);
        }
        return Env.Call("payment.transaction", "ComputeReference", providerCode, prefix, separator, args); 
    }

    public virtual string ComputeReferencePrefix(string providerCode, string separator, params object[] args) 
    { 
        // implement logic to compute reference prefix
        return ""; 
    }

    public virtual Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues) 
    { 
        if (this.ProviderCode != "payulatam") 
        {
            return Env.Call("payment.transaction", "GetSpecificRenderingValues", processingValues); 
        }

        string apiUrl = this.ProviderId.State == "enabled" ? "https://checkout.payulatam.com/ppp-web-gateway-payu/" : "https://sandbox.checkout.payulatam.com/ppp-web-gateway-payu/";
        string baseUrl = this.ProviderId.GetBaseUrl();
        Dictionary<string, object> payulatamValues = new Dictionary<string, object> 
        {
            { "merchantId", this.ProviderId.PayulatamMerchantId },
            { "referenceCode", this.Reference },
            { "description", this.Reference },
            { "amount", Math.Round(processingValues["amount"], this.CurrencyId.DecimalPlaces ?? 2) },
            { "tax", 0 },
            { "taxReturnBase", 0 },
            { "currency", this.CurrencyId.Name },
            { "paymentMethods", Const.PAYMENT_METHODS_MAPPING.GetValueOrDefault(this.PaymentMethodCode, this.PaymentMethodCode) },
            { "accountId", this.ProviderId.PayulatamAccountId },
            { "buyerFullName", this.PartnerName },
            { "buyerEmail", this.PartnerEmail },
            { "responseUrl", urls.UrlJoin(baseUrl, PayuLatamController.ReturnUrl) },
            { "confirmationUrl", urls.UrlJoin(baseUrl, PayuLatamController.WebhookUrl) },
            { "api_url", apiUrl },
        };
        if (this.ProviderId.State != "enabled") 
        {
            payulatamValues["test"] = 1;
        }
        payulatamValues["signature"] = this.ProviderId.PayulatamGenerateSign(payulatamValues, false);
        return payulatamValues;
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData) 
    { 
        if (providerCode != "payulatam") 
        {
            return Env.Call("payment.transaction", "GetTxFromNotificationData", providerCode, notificationData);
        }
        string reference = notificationData.GetValueOrDefault("referenceCode") as string;
        return Env.Search<PaymentTransaction>(x => x.Reference == reference && x.ProviderCode == "payulatam").FirstOrDefault();
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData) 
    { 
        Env.Call("payment.transaction", "ProcessNotificationData", notificationData);
        if (this.ProviderCode != "payulatam") 
        {
            return;
        }

        this.ProviderReference = notificationData.GetValueOrDefault("transactionId") as string;

        string paymentMethodType = notificationData.GetValueOrDefault("lapPaymentMethod") as string;
        if (!string.IsNullOrEmpty(paymentMethodType)) 
        {
            paymentMethodType = paymentMethodType.ToLower();
        }
        Payment.PaymentMethod paymentMethod = Env.Call<Payment.PaymentMethod>("payment.method", "_get_from_code", paymentMethodType);
        this.PaymentMethodId = paymentMethod ?? this.PaymentMethodId;

        string status = notificationData.GetValueOrDefault("lapTransactionState") as string;
        string stateMessage = notificationData.GetValueOrDefault("message") as string;
        if (status == "PENDING") 
        {
            this.SetPending();
        }
        else if (status == "APPROVED") 
        {
            this.SetDone(new string[] { "cancel" });
        }
        else if (status == "EXPIRED" || status == "DECLINED") 
        {
            this.SetCanceled(stateMessage);
        }
        else 
        {
            _logger.Warning($"received data with invalid payment status ({status}) for transaction with reference {this.Reference}");
            this.SetError("PayU Latam: Invalid payment status.");
        }
    }

    public virtual void SetPending() 
    { 
        // implement logic to set pending state
    }

    public virtual void SetDone(string[] extraAllowedStates = null) 
    { 
        // implement logic to set done state
    }

    public virtual void SetCanceled(string stateMessage = null) 
    { 
        // implement logic to set canceled state
    }

    public virtual void SetError(string errorMessage) 
    { 
        // implement logic to set error state
    }
}
