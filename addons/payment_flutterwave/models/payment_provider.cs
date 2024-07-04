csharp
public partial class PaymentProvider 
{
    public virtual void ComputeFeatureSupportFields() 
    {
        if (this.Code == "flutterwave") 
        {
            this.SupportTokenization = true;
        }
    }

    public virtual List<PaymentProvider> GetCompatibleProviders(bool IsValidation, Report Report) 
    {
        var providers = Env.Call<List<PaymentProvider>>("payment.provider", "_get_compatible_providers", this, IsValidation, Report);
        if (IsValidation) 
        {
            var unfilteredProviders = providers;
            providers = providers.Where(p => p.Code != "flutterwave").ToList();
            Env.Call("payment.provider", "add_to_report", Report, unfilteredProviders.Except(providers).ToList(), false, "validation_not_supported");
        }
        return providers;
    }

    public virtual List<Currency> GetSupportedCurrencies() 
    {
        var supportedCurrencies = Env.Call<List<Currency>>("payment.provider", "_get_supported_currencies", this);
        if (this.Code == "flutterwave") 
        {
            supportedCurrencies = supportedCurrencies.Where(c => c.Name.IsIn(const.SUPPORTED_CURRENCIES)).ToList();
        }
        return supportedCurrencies;
    }

    public virtual Dictionary<string, object> FlutterwaveMakeRequest(string Endpoint, Dictionary<string, object> Payload, string Method = "POST") 
    {
        var url = $"https://api.flutterwave.com/v3/{Endpoint}";
        var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {this.FlutterwaveSecretKey}" } };
        try 
        {
            if (Method == "GET") 
            {
                var response = Env.Call<Dictionary<string, object>>("http", "request", "get", url, Payload, headers);
                if (response.ContainsKey("status_code") && (int)response["status_code"] != 200) 
                {
                    throw new Exception($"Invalid API request at {url} with data:\n{Payload}");
                }
                return response;
            } 
            else 
            {
                var response = Env.Call<Dictionary<string, object>>("http", "request", "post", url, Payload, headers);
                if (response.ContainsKey("status_code") && (int)response["status_code"] != 200) 
                {
                    throw new Exception($"Invalid API request at {url} with data:\n{Payload}");
                }
                return response;
            }
        } 
        catch (Exception ex) 
        {
            throw new Exception($"Flutterwave: The communication with the API failed. Flutterwave gave us the following information: '{ex.Message}'");
        }
    }

    public virtual List<string> GetDefaultPaymentMethodCodes() 
    {
        var defaultCodes = Env.Call<List<string>>("payment.provider", "_get_default_payment_method_codes", this);
        if (this.Code != "flutterwave") 
        {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
