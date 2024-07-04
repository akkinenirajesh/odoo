csharp
public partial class PaymentProvider {
    public virtual PaymentProvider GetCompatibleProviders(int companyId, int? saleOrderId, int? websiteId, Dictionary<string, object> kwargs) {
        var compatibleProviders = Env.Call<PaymentProvider>("_get_compatible_providers", companyId, saleOrderId, websiteId, kwargs);
        var order = Env.Call<SaleOrder>("Browse", saleOrderId).Exists();

        if (order.GetFieldValue("CarrierId").GetFieldValue("DeliveryType") != "Onsite" || !order.GetFieldValue("OrderLine").Any(x => x.GetFieldValue("ProductId").GetFieldValue("Type") == "Consu")) {
            var unfilteredProviders = compatibleProviders;
            compatibleProviders = compatibleProviders.Where(p => p.GetFieldValue("Code") != "custom" || p.CustomMode != "Onsite").ToList();
            Env.Call<PaymentUtils>("AddToReport", unfilteredProviders.Except(compatibleProviders).ToList(), new Dictionary<string, object>() { { "Available", false }, { "Reason", "no onsite picking carriers available" } });
        }

        return compatibleProviders;
    }

    public virtual List<string> GetDefaultPaymentMethodCodes() {
        var defaultCodes = Env.Call<PaymentProvider>("_get_default_payment_method_codes");
        if (CustomMode != "Onsite") {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
