csharp
public partial class PosPaymentMethod
{
    public string PaytmTid { get; set; }
    public string ChannelId { get; set; }
    public string AcceptPayment { get; set; }
    public string AllowedPaymentModes { get; set; }
    public string PaytmMid { get; set; }
    public string PaytmMerchantKey { get; set; }
    public bool PaytmTestMode { get; set; }

    public virtual List<PosPaymentMethod> GetPaymentTerminalSelection()
    {
        var selection = Env.Call<List<PosPaymentMethod>>("Pos.PosPaymentMethod", "_GetPaymentTerminalSelection");
        selection.Add(new PosPaymentMethod { Id = "paytm", Name = "PayTM" });
        return selection;
    }

    public virtual Dictionary<string, object> PaytmMakeRequest(string url, Dictionary<string, object> payload = null)
    {
        // ...
    }

    public virtual Dictionary<string, object> PaytmMakePaymentRequest(decimal amount, string transactionId, string referenceId, long timestamp)
    {
        // ...
    }

    public virtual Dictionary<string, object> PaytmFetchPaymentStatus(string transactionId, string referenceId, long timestamp)
    {
        // ...
    }

    public virtual string PaytmGenerateSignature(Dictionary<string, object> paramsDict, string key)
    {
        // ...
    }

    public virtual Dictionary<string, object> PaytmGetRequestBody(string transactionId, string referenceId, long timestamp)
    {
        // ...
    }

    public virtual Dictionary<string, object> PaytmGetRequestHead(Dictionary<string, object> body)
    {
        // ...
    }

    public virtual void CheckPaytmTerminal()
    {
        if (this.UsePaymentTerminal == "paytm" && Env.Call<Currency>("res.currency", "GetCurrency", this.CompanyId).Name != "INR")
        {
            throw new UserError("This Payment Terminal is only valid for INR Currency");
        }
    }
}
