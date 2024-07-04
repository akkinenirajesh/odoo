csharp
public partial class PaymentProvider
{
    public void ComputeFeatureSupportFields()
    {
        if (this.Code == "ogone")
        {
            this.SupportTokenization = true;
        }
        base.ComputeFeatureSupportFields();
    }

    public List<PaymentProvider> GetCompatibleProviders(bool IsValidation, Report Report)
    {
        var providers = base.GetCompatibleProviders(IsValidation, Report);
        if (IsValidation)
        {
            var unfilteredProviders = providers;
            providers = providers.Where(p => p.Code != "ogone").ToList();
            Report.Add(unfilteredProviders.Except(providers).ToList(), false, ReportReasonsMapping.ValidationNotSupported);
        }
        return providers;
    }

    public string OgoneGetApiUrl(string ApiKey)
    {
        if (this.State == "enabled")
        {
            if (ApiKey == "hosted_payment_page")
            {
                return "https://secure.ogone.com/ncol/prod/orderstandard_utf8.asp";
            }
            else if (ApiKey == "directlink")
            {
                return "https://secure.ogone.com/ncol/prod/orderdirect_utf8.asp";
            }
        }
        else if (this.State == "test")
        {
            if (ApiKey == "hosted_payment_page")
            {
                return "https://ogone.test.v-psp.com/ncol/test/orderstandard_utf8.asp";
            }
            else if (ApiKey == "directlink")
            {
                return "https://ogone.test.v-psp.com/ncol/test/orderdirect_utf8.asp";
            }
        }
        return null;
    }

    public string OgoneGenerateSignature(Dictionary<string, string> Values, bool Incoming, bool FormatKeys)
    {
        Func<string, bool> filterKey = _key => !Incoming || const.VALID_KEYS.Contains(_key);
        string key = Incoming ? this.OgoneShakeyOut : this.OgoneShakeyIn;
        var formattedItems = FormatKeys ? Values.Select(v => (v.Key.ToUpper().Replace('_', '.'), v.Value)).ToList() : Values.Select(v => (v.Key.ToUpper(), v.Value)).ToList();
        var sortedItems = formattedItems.OrderBy(i => i.Key).ToList();
        string signingString = string.Join("", sortedItems.Where(i => filterKey(i.Key) && !string.IsNullOrEmpty(i.Value)).Select(i => $"{i.Key}={i.Value}{key}"));
        using var shasign = HashAlgorithm.Create(this.OgoneHashFunction);
        var hashBytes = shasign.ComputeHash(Encoding.UTF8.GetBytes(signingString));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public byte[] OgoneMakeRequest(Dictionary<string, string> Payload = null, string Method = "POST")
    {
        var url = this.OgoneGetApiUrl("directlink");
        try
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(Payload);
            var response = client.PostAsync(url, content).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsByteArrayAsync().Result;
        }
        catch (HttpRequestException ex)
        {
            Env.Logger.Error($"unable to reach endpoint at {url}");
            throw new ValidationError("Ogone: " + Env.Translate("Could not establish the connection to the API."));
        }
        catch (Exception ex)
        {
            Env.Logger.Error($"invalid API request at {url} with data {Payload}");
            throw new ValidationError("Ogone: " + Env.Translate("The communication with the API failed."));
        }
    }

    public List<string> GetDefaultPaymentMethodCodes()
    {
        var defaultCodes = base.GetDefaultPaymentMethodCodes();
        if (this.Code != "ogone")
        {
            return defaultCodes;
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }
}
