C#
public partial class PaymentTransaction
{
    public string Reference { get; set; }
    public string ProviderCode { get; set; }
    public string ProviderReference { get; set; }
    public string PaymentMethodCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public string State { get; set; }
    public DateTime Date { get; set; }
    public Partner Partner { get; set; }
    public Invoice PartnerInvoice { get; set; }
    public Order PartnerOrder { get; set; }
    public Acquirer Acquirer { get; set; }

    public void ComputeReference(string providerCode, string prefix, string separator)
    {
        if (providerCode != "asiapay")
        {
            // Call base implementation
            return;
        }

        if (string.IsNullOrEmpty(prefix))
        {
            prefix = this.ComputeReferencePrefix(providerCode, separator);
        }

        prefix = PaymentUtils.SingularizeReferencePrefix(prefix, 35);
        // Call base implementation with sanitized prefix
        return;
    }

    private string ComputeReferencePrefix(string providerCode, string separator)
    {
        // Implement logic to compute the prefix based on provider code, separator, and other context
        return "";
    }

    public Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues)
    {
        if (this.ProviderCode != "asiapay")
        {
            // Call base implementation
            return processingValues;
        }

        string base_url = this.Acquirer.GetBaseUrl();
        string lang = Env.Context.Get("lang") ?? "en_US";
        Dictionary<string, object> renderingValues = new Dictionary<string, object>()
        {
            { "merchant_id", this.Acquirer.AsiaPayMerchantId },
            { "amount", this.Amount },
            { "reference", this.Reference },
            { "currency_code", Const.CURRENCY_MAPPING[this.Currency.Name] },
            { "mps_mode", "SCP" },
            { "return_url", urls.UrlJoin(base_url, AsiaPayController.ReturnUrl) },
            { "payment_type", "N" },
            { "language", GetLanguageCode(lang) },
            { "payment_method", Const.PAYMENT_METHODS_MAPPING.TryGetValue(this.PaymentMethod.Code, out string code) ? code : "ALL" },
        };
        renderingValues.Add("secure_hash", this.Acquirer.CalculateSignature(renderingValues, false));
        renderingValues.Add("api_url", this.Acquirer.GetApiUrl());
        return renderingValues;
    }

    private string GetLanguageCode(string lang)
    {
        // Implement logic to map the language code based on provided lang
        string languageCode = Const.LANGUAGE_CODES_MAPPING.TryGetValue(lang, out string code) ? code : Const.LANGUAGE_CODES_MAPPING["en"];
        return languageCode;
    }

    public PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        if (providerCode != "asiapay")
        {
            // Call base implementation
            return Env.Ref<PaymentTransaction>(notificationData);
        }

        string reference = (string)notificationData["Ref"];
        if (string.IsNullOrEmpty(reference))
        {
            throw new ValidationError("AsiaPay: Received data with missing reference " + reference);
        }

        PaymentTransaction tx = Env.Search<PaymentTransaction>(x => x.Reference == reference && x.ProviderCode == "asiapay");
        if (tx == null)
        {
            throw new ValidationError("AsiaPay: No transaction found matching reference " + reference);
        }

        return tx;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        if (this.ProviderCode != "asiapay")
        {
            // Call base implementation
            return;
        }

        this.ProviderReference = (string)notificationData["PayRef"];
        string paymentMethodCode = (string)notificationData["payMethod"];
        PaymentMethod paymentMethod = Env.Ref<PaymentMethod>(paymentMethodCode, Const.PAYMENT_METHODS_MAPPING);
        this.PaymentMethod = paymentMethod ?? this.PaymentMethod;

        string successCode = (string)notificationData["successcode"];
        string primaryResponseCode = (string)notificationData["prc"];
        if (string.IsNullOrEmpty(successCode))
        {
            throw new ValidationError("AsiaPay: Received data with missing success code.");
        }

        if (Const.SUCCESS_CODE_MAPPING["done"].Contains(successCode))
        {
            this.SetDone();
        }
        else if (Const.SUCCESS_CODE_MAPPING["error"].Contains(successCode))
        {
            this.SetError($"An error occurred during the processing of your payment (success code {successCode}; primary response code {primaryResponseCode}). Please try again.");
        }
        else
        {
            Env.Logger.Warning($"Received data with invalid success code ({successCode}) for transaction with primary response code {primaryResponseCode} and reference {this.Reference}.");
            this.SetError($"AsiaPay: Unknown success code: {successCode}");
        }
    }

    private void SetDone()
    {
        this.State = "done";
    }

    private void SetError(string message)
    {
        this.State = "error";
        // Handle the error message based on the implementation
    }
}
