C#
public partial class PaymentProvider {
    public string GetSupportedCurrencies() {
        // Override of `payment` to return the supported currencies. 
        var supportedCurrencies = Env.Call("payment.provider", "_get_supported_currencies", this);
        if (this.Code == "sips") {
            supportedCurrencies = Env.Call("payment.provider", "filtered", supportedCurrencies, lambda c: c.Name in PaymentSipsConst.SUPPORTED_CURRENCIES.Keys);
        }
        return supportedCurrencies;
    }

    public string SipsGenerateShasign(string data) {
        // Generate the shasign for incoming or outgoing communications.
        // Note: self.ensure_one()
        var key = this.SipsSecret;
        var shasign = Env.Call("hashlib.sha256", "hexdigest", (data + key).Encode("utf-8"));
        return shasign;
    }

    public string GetDefaultPaymentMethodCodes() {
        // Override of `payment` to return the default payment method codes. 
        var defaultCodes = Env.Call("payment.provider", "_get_default_payment_method_codes", this);
        if (this.Code != "sips") {
            return defaultCodes;
        }
        return PaymentSipsConst.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
