csharp
public partial class PaymentProvider
{
    public PaymentProvider()
    {
    }

    public List<Core.Currency> GetSupportedCurrencies()
    {
        List<Core.Currency> supportedCurrencies =  Env.Call<List<Core.Currency>>("payment.payment.provider", "_get_supported_currencies");
        if (this.Code == "mollie")
        {
            supportedCurrencies = supportedCurrencies.Where(c => c.Name.IsIn(const.SUPPORTED_CURRENCIES)).ToList();
        }
        return supportedCurrencies;
    }

    public Dictionary<string, object> MollieMakeRequest(string endpoint, Dictionary<string, object> data = null, string method = "POST")
    {
        endpoint = $"/v2/{endpoint.Trim('/')}";
        string url = $"https://api.mollie.com/{endpoint}";

        string odooVersion = Env.Call<string>("service.common", "exp_version")["server_version"];
        string moduleVersion = Env.Call<string>("base.module_payment_mollie", "installed_version");
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            {"Accept", "application/json"},
            {"Authorization", $"Bearer {this.MollieApiKey}"},
            {"Content-Type", "application/json"},
            {"User-Agent", $"Odoo/{odooVersion} MollieNativeOdoo/{moduleVersion}"}
        };

        try
        {
            var response =  Env.Call<Dictionary<string, object>>("requests", "request", method, url, data, headers, 60);
            if (response["status_code"] != 200)
            {
                throw new Exception($"Mollie: The communication with the API failed. Mollie gave us the following information: {response["detail"]}");
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unable to reach endpoint at {url}");
            throw new Exception($"Mollie: Could not establish the connection to the API.");
        }
    }

    public List<string> GetDefaultPaymentMethodCodes()
    {
        List<string> defaultCodes =  Env.Call<List<string>>("payment.payment.provider", "_get_default_payment_method_codes");
        if (this.Code != "mollie")
        {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
