csharp
public partial class PaymentProvider
{
    public string _GetDefaultPaymentMethodCodes()
    {
        if (this.Code != "aps")
        {
            return Env.Call("payment.provider", "_GetDefaultPaymentMethodCodes", this);
        }
        return const.DEFAULT_PAYMENT_METHOD_CODES;
    }

    public string _ApsCalculateSignature(Dictionary<string, string> data, bool incoming = true)
    {
        var signData = string.Join("", data.Where(x => x.Key != "signature").OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        var key = incoming ? this.ApsShaResponse : this.ApsShaRequest;
        var signingString = string.Join("", key, signData, key);
        return System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingString)).Aggregate("", (s, b) => s + b.ToString("x2"));
    }

    public string _ApsGetApiUrl()
    {
        if (this.State == "enabled")
        {
            return "https://checkout.payfort.com/FortAPI/paymentPage";
        }
        return "https://sbcheckout.payfort.com/FortAPI/paymentPage";
    }
}
