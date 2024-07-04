C#
public partial class PaymentProvider {

    public void ComputeFeatureSupportFields() {
        if (this.Code == "adyen") {
            this.SupportManualCapture = "partial";
            this.SupportRefund = "partial";
            this.SupportTokenization = true;
        }
    }

    public void AdyenExtractPrefixFromApiUrl(string AdyenApiUrlPrefix) {
        this.AdyenApiUrlPrefix = System.Text.RegularExpressions.Regex.Replace(AdyenApiUrlPrefix, @"(?:https://)?(\w+-\w+).*", @"$1");
    }

    public object AdyenMakeRequest(string Endpoint, string EndpointParam = null, object Payload = null, string Method = "POST", string IdempotencyKey = null) {
        // Implement the logic for Adyen API request based on the parameters.
        // This will include building the URL, setting headers, and handling the response.
        return null; // Replace with actual response.
    }

    public string AdyenComputeShopperReference(int PartnerId) {
        return $"ODOO_PARTNER_{PartnerId}";
    }

    public string AdyenGetInlineFormValues(string PmCode, double Amount = 0, string Currency = null) {
        // Implement logic to construct the JSON object with inline form values.
        return null; // Replace with actual JSON.
    }

    public object AdyenGetFormattedAmount(double Amount = 0, string Currency = null) {
        // Implement logic to format the amount based on currency.
        return null; // Replace with actual formatted amount.
    }

    public List<string> GetDefaultPaymentMethodCodes() {
        var defaultCodes = Env.Call("PaymentProvider", "_get_default_payment_method_codes");
        if (this.Code != "adyen") {
            return defaultCodes;
        }
        return new List<string>() { "adyen" };
    }
}
