csharp
public partial class PaymentProvider {
    public virtual PaymentProvider GetDefaultPaymentMethodCodes() {
        var defaultCodes = Env.CallMethod<PaymentProvider>("_get_default_payment_method_codes", this);
        if (this.Code != "buckaroo") {
            return defaultCodes;
        }
        return Env.Const.DEFAULT_PAYMENT_METHOD_CODES;
    }

    public virtual IEnumerable<Currency> GetSupportedCurrencies() {
        var supportedCurrencies = Env.CallMethod<IEnumerable<Currency>>("_get_supported_currencies", this);
        if (this.Code == "buckaroo") {
            supportedCurrencies = supportedCurrencies.Where(c => Env.Const.SUPPORTED_CURRENCIES.Contains(c.Name));
        }
        return supportedCurrencies;
    }

    public virtual string BuckarooGenerateDigitalSign(Dictionary<string, string> values, bool incoming = true) {
        if (incoming) {
            // Incoming communication values must be URL-decoded before checking the signature. The
            // key 'brq_signature' must be ignored.
            var items = values.Select(kvp => new KeyValuePair<string, string>(kvp.Key, System.Net.WebUtility.UrlDecode(kvp.Value))).Where(kvp => kvp.Key.ToLower() != "brq_signature").ToList();
        } else {
            var items = values.ToList();
        }

        var filteredItems = items.Where(kvp => kvp.Key.StartsWith("add_", StringComparison.OrdinalIgnoreCase) || kvp.Key.StartsWith("brq_", StringComparison.OrdinalIgnoreCase) || kvp.Key.StartsWith("cust_", StringComparison.OrdinalIgnoreCase)).ToList();
        var sortedItems = filteredItems.OrderBy(kvp => kvp.Key.ToLower()).ToList();

        var signString = string.Join("", sortedItems.Select(kvp => $"{kvp.Key}={kvp.Value ?? ""}")).ToList();
        signString += this.BuckarooSecretKey;

        return System.Security.Cryptography.SHA1.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(signString)).Aggregate("", (s, b) => s + b.ToString("x2"));
    }

    public virtual string BuckarooGetApiUrl() {
        if (this.State == "enabled") {
            return "https://checkout.buckaroo.nl/html/";
        }
        return "https://testcheckout.buckaroo.nl/html/";
    }
}
