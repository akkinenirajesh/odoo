csharp
public partial class PaymentProvider {
    public virtual string GetSupportedCurrencies() {
        string supportedCurrencies = this.Env.Call("payment.provider", "_get_supported_currencies");
        if (this.Code == "paypal") {
            supportedCurrencies = this.Env.Call("payment.provider", "_filter_supported_currencies", supportedCurrencies, const.SUPPORTED_CURRENCIES);
        }
        return supportedCurrencies;
    }

    public virtual string GetPaypalApiUrl() {
        if (this.State == "enabled") {
            return "https://www.paypal.com/cgi-bin/webscr";
        }
        else {
            return "https://www.sandbox.paypal.com/cgi-bin/webscr";
        }
    }

    public virtual string GetDefaultPaymentMethodCodes() {
        if (this.Code != "paypal") {
            return this.Env.Call("payment.provider", "_get_default_payment_method_codes");
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
