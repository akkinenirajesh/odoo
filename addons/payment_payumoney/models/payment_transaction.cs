csharp
public partial class PaymentTransaction
{
    public string ProviderCode { get; set; }
    public string Reference { get; set; }
    public double Amount { get; set; }
    public string PartnerEmail { get; set; }
    public string PartnerPhone { get; set; }
    public string ProviderReference { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Partner Partner { get; set; }

    public Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (ProviderCode != "payumoney")
        {
            return processingValues;
        }

        string[] names = Env.SplitPartnerName(Partner.Name);
        string firstName = names[0];
        string lastName = names[1];

        string apiUrl = "https://secure.payu.in/_payment";
        if (Env.GetPaymentMethod(ProviderCode).State != "enabled")
        {
            apiUrl = "https://sandboxsecure.payu.in/_payment";
        }

        Dictionary<string, object> payumoneyValues = new Dictionary<string, object>
        {
            { "key", Env.GetPaymentMethod(ProviderCode).PayUMoneyMerchantKey },
            { "txnid", Reference },
            { "amount", Amount },
            { "productinfo", Reference },
            { "firstname", firstName },
            { "lastname", lastName },
            { "email", PartnerEmail },
            { "phone", PartnerPhone },
            { "return_url", Env.UrlJoin(Env.GetBaseUrl(), PayUMoneyController.ReturnUrl) },
            { "api_url", apiUrl },
        };

        payumoneyValues["hash"] = Env.GetPaymentMethod(ProviderCode).GeneratePayUMoneySign(payumoneyValues, false);
        return payumoneyValues;
    }

    public PaymentTransaction GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "payumoney")
        {
            return Env.GetTransactionFromNotificationData(providerCode, notificationData);
        }

        string reference = notificationData["txnid"] as string;
        if (reference == null)
        {
            throw new ValidationError("PayUmoney: Received data with missing reference.");
        }

        PaymentTransaction transaction = Env.SearchPaymentTransaction(new List<Tuple<string, object>> { Tuple.Create("Reference", reference), Tuple.Create("ProviderCode", "payumoney") });
        if (transaction == null)
        {
            throw new ValidationError("PayUmoney: No transaction found matching reference " + reference + ".");
        }

        return transaction;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (ProviderCode != "payumoney")
        {
            return;
        }

        ProviderReference = notificationData["payuMoneyId"] as string;

        string paymentMethodType = notificationData["bankcode"] as string;
        PaymentMethod paymentMethod = Env.GetPaymentMethod(paymentMethodType);
        if (paymentMethod != null)
        {
            PaymentMethod = paymentMethod;
        }

        string status = notificationData["status"] as string;
        if (status == "success")
        {
            SetDone();
        }
        else
        {
            string errorCode = notificationData["Error"] as string;
            SetError("PayUmoney: The payment encountered an error with code " + errorCode);
        }
    }

    private void SetDone()
    {
        // Implementation for setting transaction state to 'Done'
    }

    private void SetError(string errorMessage)
    {
        // Implementation for setting transaction state to 'Error' with the provided message
    }
}
