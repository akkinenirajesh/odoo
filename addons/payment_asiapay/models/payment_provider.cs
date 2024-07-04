csharp
public partial class PaymentProvider
{
    public string GetApiUrl()
    {
        if (this.State == "enabled")
        {
            return Env.Get("const.API_URLS.production").Get(this.AsiapayBrand).ToString();
        }
        else
        {
            return Env.Get("const.API_URLS.test").Get(this.AsiapayBrand).ToString();
        }
    }

    public string CalculateSignature(Dictionary<string, string> data, bool incoming)
    {
        List<string> dataToSign = new List<string>();
        if (incoming)
        {
            dataToSign = Env.Get("const.SIGNATURE_KEYS.incoming").Select(k => data[k]).ToList();
        }
        else
        {
            dataToSign = Env.Get("const.SIGNATURE_KEYS.outgoing").Select(k => data[k]).ToList();
        }

        dataToSign.Add(this.AsiapaySecureHashSecret);
        string signingString = string.Join("|", dataToSign);
        HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.AsiapaySecureHashFunction);
        byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(signingString));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public List<string> GetDefaultPaymentMethodCodes()
    {
        if (this.Code != "asiapay")
        {
            return Env.Call<List<string>>("payment.paymentprovider", "_get_default_payment_method_codes", this);
        }
        return Env.Get("const.DEFAULT_PAYMENT_METHOD_CODES");
    }

    public void LimitAvailableCurrencyIds()
    {
        if (this.Code == "asiapay" && this.AvailableCurrencyIds.Count > 1 && this.State != "disabled")
        {
            throw new ValidationException(_("Only one currency can be selected by AsiaPay account."));
        }
    }
}
