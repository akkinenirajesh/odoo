csharp
public partial class PaymentProvider {
    public void ComputeFeatureSupportFields() {
        if (this.Code == "demo") {
            this.SupportExpressCheckout = true;
            this.SupportManualCapture = "partial";
            this.SupportRefund = "partial";
            this.SupportTokenization = true;
        }
    }

    public void CheckProviderState() {
        if (this.Code == "demo" && this.State != "test" && this.State != "disabled") {
            throw new Exception("Demo providers should never be enabled.");
        }
    }

    public List<string> GetDefaultPaymentMethodCodes() {
        List<string> defaultCodes = Env.GetDefaultPaymentMethodCodes();
        if (this.Code != "demo") {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
