csharp
public partial class PaymentProvider
{
    public virtual List<PaymentProvider> GetCompatibleProviders(int currencyId, object report)
    {
        List<PaymentProvider> providers = Env.CallMethod<List<PaymentProvider>>("payment.payment_provider", "_get_compatible_providers", new object[] { currencyId, report });

        if (currencyId != null)
        {
            var currency = Env.GetRecord<ResCurrency>(currencyId);
            if (currency.Name != "CNY")
            {
                var unfilteredProviders = providers;
                providers = providers.Where(p => p.Code != "alipay" || p.AlipayPaymentMethod != "express_checkout").ToList();
                Env.CallMethod("payment.payment_utils", "add_to_report", new object[] { report, unfilteredProviders.Except(providers), false, "incompatible currency; only CNY is supported" });
            }
        }

        return providers;
    }

    public virtual string AlipayComputeSignature(Dictionary<string, string> data)
    {
        // Rearrange parameters in the data set alphabetically
        var dataToSign = data.OrderBy(x => x.Key).ToList();
        // Format key-value pairs of parameters that should be signed
        dataToSign = dataToSign.Where(k => k.Key != "sign" && k.Key != "sign_type" && k.Key != "reference").Select(k => $"{k.Key}={k.Value}").ToList();
        // Build the data string of &-separated key-value pairs
        var dataString = string.Join("&", dataToSign);
        dataString += this.AlipayMd5SignatureKey;
        return System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(dataString)).Aggregate("", (s, e) => s + e.ToString("x2"));
    }

    public virtual string AlipayGetApiUrl()
    {
        if (this.State == "enabled")
        {
            return "https://mapi.alipay.com/gateway.do";
        }
        else
        {
            return "https://openapi.alipaydev.com/gateway.do";
        }
    }

    public virtual List<string> GetDefaultPaymentMethodCodes()
    {
        var defaultCodes = Env.CallMethod<List<string>>("payment.payment_provider", "_get_default_payment_method_codes");
        if (this.Code != "alipay")
        {
            return defaultCodes;
        }
        return new List<string>() { "express_checkout", "standard_checkout" };
    }
}
