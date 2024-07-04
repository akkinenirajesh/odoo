csharp
public partial class PaymentProvider
{
    public virtual void ComputeFeatureSupportFields()
    {
        if (this.Code == "razorpay")
        {
            this.SupportManualCapture = "full_only";
            this.SupportRefund = "partial";
            this.SupportTokenization = true;
        }
    }

    public virtual List<Currency> GetSupportedCurrencies()
    {
        var supportedCurrencies = Env.GetService<CurrencyService>().GetSupportedCurrencies();
        if (this.Code == "razorpay")
        {
            supportedCurrencies = supportedCurrencies.Where(c => c.Name in const.SUPPORTED_CURRENCIES).ToList();
        }
        return supportedCurrencies;
    }

    public virtual Dictionary<string, object> RazorpayMakeRequest(string endpoint, Dictionary<string, object> payload = null, string method = "POST")
    {
        var url = UrlJoin("https://api.razorpay.com/v1/", endpoint);
        var auth = new Tuple<string, string>(this.RazorpayKeyId, this.RazorpayKeySecret);
        try
        {
            if (method == "GET")
            {
                var response = Requests.Get(url, auth, payload, 10);
                if (!response.IsSuccessStatusCode)
                {
                    throw new ValidationError($"Razorpay: {response.Content}");
                }
                return response.Content.ToObject<Dictionary<string, object>>();
            }
            else
            {
                var response = Requests.Post(url, auth, payload, 10);
                if (!response.IsSuccessStatusCode)
                {
                    throw new ValidationError($"Razorpay: {response.Content}");
                }
                return response.Content.ToObject<Dictionary<string, object>>();
            }
        }
        catch (Exception ex)
        {
            throw new ValidationError($"Razorpay: {ex.Message}");
        }
    }

    public virtual string RazorpayCalculateSignature(byte[] data)
    {
        var secret = this.RazorpayWebhookSecret;
        return HmacSha256(secret, data).ToHex();
    }

    public virtual List<string> GetDefaultPaymentMethodCodes()
    {
        if (this.Code != "razorpay")
        {
            return Env.GetService<PaymentService>().GetDefaultPaymentMethodCodes();
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }

    public virtual decimal GetValidationAmount()
    {
        if (this.Code != "razorpay")
        {
            return Env.GetService<PaymentService>().GetValidationAmount();
        }
        return 1.0m;
    }
}
