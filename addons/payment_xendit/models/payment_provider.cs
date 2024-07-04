csharp
public partial class PaymentProvider {

    public void ComputeFeatureSupportFields() {
        // Override of `payment` to enable additional features.
        this.SupportTokenization = (this.Code == "xendit");
    }

    public List<Currency> GetSupportedCurrencies() {
        // Override of `payment` to return the supported currencies.
        List<Currency> supportedCurrencies = base.GetSupportedCurrencies();
        if (this.Code == "xendit") {
            supportedCurrencies = supportedCurrencies.Where(c => const.SUPPORTED_CURRENCIES.Contains(c.Name)).ToList();
        }
        return supportedCurrencies;
    }

    public List<string> GetDefaultPaymentMethodCodes() {
        // Override of `payment` to return the default payment method codes.
        List<string> defaultCodes = base.GetDefaultPaymentMethodCodes();
        if (this.Code != "xendit") {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }

    public Dictionary<string, object> XenditMakeRequest(string endpoint, Dictionary<string, object> payload = null) {
        // Make a request to Xendit API and return the JSON-formatted content of the response.

        // Note: self.ensure_one()

        // :param str endpoint: The endpoint to be reached by the request.
        // :param dict payload: The payload of the request.
        // :return The JSON-formatted content of the response.
        // :rtype: dict
        // :raise ValidationError: If an HTTP error occurs.

        string url = $"https://api.xendit.co/{endpoint}";
        try {
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.XenditSecretKey}:")));
                var response = client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")).Result;
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
            }
        } catch (Exception ex) {
            _logger.Error(ex, "Unable to reach endpoint at {0}", url);
            throw new ValidationError($"Xendit: {Env.Translate("Could not establish the connection to the API.")}");
        }
    }

    public ir.ui.view GetRedirectFormView(bool isValidation = false) {
        // Override of `payment` to avoid rendering the form view for validation operations.

        // Unlike other compatible payment methods in Xendit, `Card` is implemented using a direct
        // flow. To avoid rendering a useless template, and also to avoid computing wrong values, this
        // method returns `None` for Xendit's validation operations (Card is and will always be the
        // sole tokenizable payment method for Xendit).

        // Note: `self.ensure_one()`

        // :param bool is_validation: Whether the operation is a validation.
        // :return: The view of the redirect form template or None.
        // :rtype: ir.ui.view | None

        if (this.Code == "xendit" && isValidation) {
            return null;
        }
        return base.GetRedirectFormView(isValidation);
    }

}
